using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Repositories
{
    public sealed class SlotRepository : ISlotRepository
    {
        private readonly BookingDbContext _db;
        public SlotRepository(BookingDbContext db) => _db = db;

        public Task<Slot?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Slots.FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<IReadOnlyList<Slot>> GetForBranchOnDateAsync(
            int branchId, DateTime dateUtc, string? serviceType, CancellationToken ct)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            IQueryable<Slot> query = _db.Slots
                .Include(s => s.Appointment)
                .Where(s => s.BranchId == branchId && s.StartsAtUtc >= start && s.StartsAtUtc < end);

            if (!string.IsNullOrWhiteSpace(serviceType))
                query = query.Where(s => s.ServiceType == serviceType);

            return await query.OrderBy(s => s.StartsAtUtc).ToListAsync(ct);
        }
    }

}
