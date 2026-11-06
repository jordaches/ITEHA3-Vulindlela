using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Repositories
{
    public sealed class BranchRepository : IBranchRepository
    {
        private readonly BookingDbContext _db;
        public BranchRepository(BookingDbContext db) => _db = db;

        public async Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken ct) =>
            await _db.Branches.OrderBy(b => b.Name).ToListAsync(ct);

        public Task<Branch?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Branches.FirstOrDefaultAsync(b => b.Id == id, ct);
    }
}
