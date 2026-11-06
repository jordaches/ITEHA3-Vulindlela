using System.Net.Http.Json;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Clients;

public sealed class ClearanceClient : IClearanceClient
{
    private readonly HttpClient _http;

    public ClearanceClient(HttpClient http) => _http = http;

    public async Task NotifyAppointmentCreatedAsync(
        int appointmentId,
        int citizenAccountId,
        string serviceType,
        CancellationToken ct)
    {
        var body = new RegisterAppointmentRequest(appointmentId, citizenAccountId, serviceType);
        using var response = await _http.PostAsJsonAsync(
            "api/clearance/appointments", body, ct);

        response.EnsureSuccessStatusCode();
    }
}