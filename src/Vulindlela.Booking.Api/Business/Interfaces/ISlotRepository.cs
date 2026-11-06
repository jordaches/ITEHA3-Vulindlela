using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot?> GetByIdAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<Slot>> GetForBranchOnDateAsync(
            int branchId, DateTime dateUtc, string? serviceType, CancellationToken ct);
    }
}
