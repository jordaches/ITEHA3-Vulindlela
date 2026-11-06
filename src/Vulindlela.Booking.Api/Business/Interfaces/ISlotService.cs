using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface ISlotService
    {
        Task<IReadOnlyList<SlotDto>> GetForBranchOnDateAsync(
            int branchId, DateTime dateUtc, string? serviceType, CancellationToken ct);
    }
}
