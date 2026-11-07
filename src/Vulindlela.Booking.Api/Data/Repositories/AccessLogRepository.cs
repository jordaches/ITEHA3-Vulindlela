using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Data.Repositories;

public sealed class AccessLogRepository : IAccessLogRepository
{
    private readonly BookingDbContext _db;
    public AccessLogRepository(BookingDbContext db) => _db = db;

    public async Task AddAsync(BookingAccessLog log, CancellationToken ct)
    {
        _db.AccessLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<BookingAccessLog>> GetForCitizenAsync(int citizenId, CancellationToken ct) =>
        await _db.AccessLogs
                 .Where(x => x.CitizenAccountId == citizenId)
                 .OrderByDescending(x => x.AccessedAtUtc)
                 .ToListAsync(ct);
}