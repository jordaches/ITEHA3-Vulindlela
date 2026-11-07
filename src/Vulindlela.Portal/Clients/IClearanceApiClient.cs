using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.Clients;

public interface IClearanceApiClient
{
    Task<IReadOnlyDictionary<int, ClearanceStatusDto>> GetStatusesAsync(
        IReadOnlyList<int> appointmentIds, CancellationToken ct);


    Task<IReadOnlyList<AccessLogEntryDto>> GetAccessLogAsync(int citizenId, CancellationToken ct);
}