using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Data.Repositories;

public sealed class ClearanceAccessLogRepository : IClearanceAccessLogRepository
{
    private readonly ClearanceDbContext _db;
    public ClearanceAccessLogRepository(ClearanceDbContext db) => _db = db;

    public async Task AddAsync(ClearanceAccessLog log, CancellationToken ct)
    {
        _db.AccessLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ClearanceAccessLog>> GetForCitizenAsync(int citizenId, CancellationToken ct) =>
        await _db.AccessLogs
                 .Where(x => x.CitizenAccountId == citizenId)
                 .OrderByDescending(x => x.AccessedAtUtc)
                 .ToListAsync(ct);
}