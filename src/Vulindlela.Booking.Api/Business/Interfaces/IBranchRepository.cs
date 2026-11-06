using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface IBranchRepository
    {
        Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken ct);
        Task<Branch?> GetByIdAsync(int id, CancellationToken ct);
    }

}
