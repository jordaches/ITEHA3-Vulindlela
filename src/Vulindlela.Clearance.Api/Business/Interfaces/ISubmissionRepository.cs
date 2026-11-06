using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business.Interfaces
{
    public interface ISubmissionRepository
    {
        Task<Submission?> GetByIdAsync(int id, CancellationToken ct);
        void Add(Submission submission);
    }
}
