using System.Net;
using System.Net.Http.Json;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.Clients;

public sealed class ClearanceApiClient : IClearanceApiClient
{
    private readonly HttpClient _http;
    public ClearanceApiClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyDictionary<int, ClearanceStatusDto>> GetStatusesAsync(
        IReadOnlyList<int> appointmentIds, CancellationToken ct)
    {
        if (appointmentIds.Count == 0)
            return new Dictionary<int, ClearanceStatusDto>();

        var tasks = appointmentIds.Select(id => FetchStatusAsync(id, ct));
        var results = await Task.WhenAll(tasks);

        return results
            .Where(r => r.status is not null)
            .ToDictionary(r => r.id, r => r.status!);
    }

    /// One appointment's clearance status, or null if Clearance has no record
    /// for it yet (404 — Booking's notification may still be in the outbox).
    /// Any other failure propagates to the caller, which decides how to degrade.
    private async Task<(int id, ClearanceStatusDto? status)> FetchStatusAsync(
        int id, CancellationToken ct)
    {
        try
        {
            var status = await _http.GetFromJsonAsync<ClearanceStatusDto>(
                $"api/clearance/appointments/{id}", ct);
            return (id, status);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return (id, null);
        }
    }
}