using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business;

public sealed class VerificationService : IVerificationService
{
    private readonly ISubmissionRepository _submissions;
    private readonly IDocumentRuleEvaluator _evaluator;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogger<VerificationService> _log;

    public VerificationService(
        ISubmissionRepository submissions,
        IDocumentRuleEvaluator evaluator,
        IUnitOfWork uow,
        IClock clock,
        ILogger<VerificationService> log)
    {
        _submissions = submissions;
        _evaluator = evaluator;
        _uow = uow;
        _clock = clock;
        _log = log;
    }

    public async Task VerifyAsync(int submissionId, CancellationToken ct)
    {
        var submission = await _submissions.GetByIdAsync(submissionId, ct);
        if (submission is null)
        {
            _log.LogWarning("Submission {Id} not found; skipping.", submissionId);
            return;
        }

        // Production equivalent: OCR, population-register lookup, virus scan.
        // The delay makes the asynchronous behaviour visible in a demo/test.
        await Task.Delay(TimeSpan.FromSeconds(3), ct);

        var status = submission.ClearanceStatus;
        var missing = await _evaluator.EvaluateAsync(
            status.ServiceType, submission.Documents.ToList(), ct);

        status.MissingDocuments.Clear();
        foreach (var m in missing) status.MissingDocuments.Add(m);

        status.State = missing.Count == 0
            ? ClearanceState.Verified
            : ClearanceState.MissingDocuments;
        status.UpdatedAtUtc = _clock.UtcNow;
        status.VerifiedAtUtc = _clock.UtcNow;

        await _uow.SaveChangesAsync(ct);
    }
}
