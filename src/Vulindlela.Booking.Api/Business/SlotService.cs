using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Business
{
    public sealed class SlotService : ISlotService
    {
        private readonly ISlotRepository _slots;
        private readonly IClock _clock;

        public SlotService(ISlotRepository slots, IClock clock)
        {
            _slots = slots;
            _clock = clock;
        }

        public async Task<IReadOnlyList<SlotDto>> GetForBranchOnDateAsync(
            int branchId, DateTime dateUtc, string? serviceType, CancellationToken ct)
        {
            var slots = await _slots.GetForBranchOnDateAsync(branchId, dateUtc, serviceType, ct);
            var now = _clock.UtcNow;

            return slots.Select(s => new SlotDto(
                s.Id,
                s.BranchId,
                s.StartsAtUtc,
                s.ServiceType,
                IsAvailable: s.Appointment is null && s.StartsAtUtc > now)).ToList();
        }
    }
}
