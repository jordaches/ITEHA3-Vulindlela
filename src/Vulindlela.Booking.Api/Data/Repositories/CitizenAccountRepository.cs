using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Repositories
{
    public sealed class CitizenAccountRepository : ICitizenAccountRepository
    {
        private readonly BookingDbContext _db;
        public CitizenAccountRepository(BookingDbContext db) => _db = db;

        public Task<CitizenAccount?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Citizens.FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<IReadOnlyList<CitizenAccount>> GetAllAsync(CancellationToken ct) =>
            await _db.Citizens.OrderBy(c => c.Id).ToListAsync(ct);

        public Task<CitizenAccount?> GetByIdNumberAsync(string idNumber, CancellationToken ct) =>
            _db.Citizens.FirstOrDefaultAsync(c => c.IdNumber == idNumber, ct);
    }
}
