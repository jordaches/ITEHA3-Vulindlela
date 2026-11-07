using Vulindlela.Shared.Contracts;

namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface ISubmissionService
    {
        Task<ClearanceStatusDto?> RegisterAppointmentAsync(RegisterAppointmentRequest req, CancellationToken ct);
        Task<SubmissionAcceptedDto?> SubmitAsync(SubmitDocumentsRequest req, CancellationToken ct);
        Task<ClearanceStatusDto?> GetStatusAsync(int appointmentId, CancellationToken ct);
        Task<(int CitizenAccountId, string State)?> GetStatusForAuthorisationAsync(int appointmentId, CancellationToken ct);
    }
}