namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface IVerificationService
    {
        Task VerifyAsync(int submissionId, CancellationToken ct);
    }
}
