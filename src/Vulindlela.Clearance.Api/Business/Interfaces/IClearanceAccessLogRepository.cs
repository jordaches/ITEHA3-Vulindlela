using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Business.Interfaces;

public interface IClearanceAccessLogRepository
{
    Task AddAsync(ClearanceAccessLog log, CancellationToken ct);
    Task<IReadOnlyList<ClearanceAccessLog>> GetForCitizenAsync(int citizenId, CancellationToken ct);
}