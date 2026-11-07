using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data.Entities;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Clearance.Api.Business
{
    public sealed class SubmissionService : ISubmissionService
    {
        private readonly IDocumentRuleRepository _rules;
        private readonly IClearanceStatusRepository _statuses;
        private readonly ISubmissionRepository _submissions;
        private readonly IDocumentQueue _queue;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;

        public SubmissionService(
            IDocumentRuleRepository rules,
            IClearanceStatusRepository statuses,
            ISubmissionRepository submissions,
            IDocumentQueue queue,
            IUnitOfWork uow,
            IClock clock)
        {
            _rules = rules;
            _statuses = statuses;
            _submissions = submissions;
            _queue = queue;
            _uow = uow;
            _clock = clock;
        }

        public async Task<ClearanceStatusDto?> RegisterAppointmentAsync(
            RegisterAppointmentRequest req, CancellationToken ct)
        {
            var existing = await _statuses.GetByAppointmentIdAsync(req.AppointmentId, ct);
            if (existing is not null) return ToDto(existing);

            var rules = await _rules.GetForServiceTypeAsync(req.ServiceType, ct);
            var state = rules.Count == 0
                ? ClearanceState.NotRequired
                : ClearanceState.AwaitingDocuments;

            var status = new ClearanceStatus
            {
                AppointmentId = req.AppointmentId,
                CitizenAccountId = req.CitizenAccountId,     // NEW
                ServiceType = req.ServiceType,
                State = state,
                CreatedAtUtc = _clock.UtcNow,
                UpdatedAtUtc = _clock.UtcNow,
                VerifiedAtUtc = state == ClearanceState.NotRequired ? _clock.UtcNow : null
            };

            _statuses.Add(status);
            await _uow.SaveChangesAsync(ct);
            return ToDto(status);
        }

        public async Task<SubmissionAcceptedDto?> SubmitAsync(
            SubmitDocumentsRequest req, CancellationToken ct)
        {
            var status = await _statuses.GetByAppointmentIdAsync(req.AppointmentId, ct);
            if (status is null)
            {
                // Registration lost (e.g. Booking notification failed). Recover.
                var rules = await _rules.GetForServiceTypeAsync(req.ServiceType, ct);
                var state = rules.Count == 0
                    ? ClearanceState.NotRequired
                    : ClearanceState.AwaitingDocuments;

                status = new ClearanceStatus
                {
                    AppointmentId = req.AppointmentId,
                    CitizenAccountId = req.CitizenAccountId,
                    ServiceType = req.ServiceType,
                    State = state,
                    CreatedAtUtc = _clock.UtcNow,
                    UpdatedAtUtc = _clock.UtcNow
                };
                _statuses.Add(status);
                await _uow.SaveChangesAsync(ct);
            }

            if (status.State is ClearanceState.PendingVerification
                                or ClearanceState.Verified
                                or ClearanceState.NotRequired)
                return null;   // controller returns 409

            var submission = new Submission
            {
                ClearanceStatusId = status.Id,
                SubmittedAtUtc = _clock.UtcNow,
                Documents = req.Documents.Select(d => new SubmittedDocument
                {
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    CertifiedOnUtc = d.CertifiedOnUtc
                }).ToList()
            };
            _submissions.Add(submission);

            status.State = ClearanceState.PendingVerification;
            status.UpdatedAtUtc = _clock.UtcNow;

            await _uow.SaveChangesAsync(ct);           // committed BEFORE enqueue
            await _queue.EnqueueAsync(submission.Id, ct);

            return new SubmissionAcceptedDto(submission.Id, status.State.ToString());
        }

        public async Task<ClearanceStatusDto?> GetStatusAsync(int appointmentId, CancellationToken ct)
        {
            var status = await _statuses.GetByAppointmentIdAsync(appointmentId, ct);
            return status is null ? null : ToDto(status);
        }

        private static ClearanceStatusDto ToDto(ClearanceStatus s) => new(
            s.AppointmentId,
            s.ServiceType,
            s.State.ToString(),
            s.MissingDocuments.Select(m => new MissingDocumentDto(m.DocumentType, m.Reason)).ToList(),
            s.VerifiedAtUtc,
            s.UpdatedAtUtc);

        public async Task<(int CitizenAccountId, string State)?> GetStatusForAuthorisationAsync(int appointmentId, CancellationToken ct)
        {
            var status = await _statuses.GetByAppointmentIdAsync(appointmentId, ct);
            return status is null ? null : (status.CitizenAccountId, status.State.ToString());
        }
    }
}
