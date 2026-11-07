using System.Net;
using System.Net.Http.Json;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Portal.Clients;

public sealed class BookingApiClient : IBookingApiClient
{
    private readonly HttpClient _http;
    public BookingApiClient(HttpClient http) => _http = http;

    public async Task<VerifyPasswordResponse?> VerifyPasswordAsync(
        string idNumber, string password, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("api/auth/verify",
            new VerifyPasswordRequest(idNumber, password), ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyPasswordResponse>(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken ct) =>
        await _http.GetFromJsonAsync<IReadOnlyList<BranchDto>>("api/branches", ct)
            ?? Array.Empty<BranchDto>();

    public async Task<IReadOnlyList<SlotDto>> GetSlotsAsync(
        int branchId, string date, string? serviceType, CancellationToken ct)
    {
        var url = $"api/slots?branchId={branchId}&date={Uri.EscapeDataString(date)}";
        if (!string.IsNullOrEmpty(serviceType))
            url += $"&serviceType={Uri.EscapeDataString(serviceType)}";

        return await _http.GetFromJsonAsync<IReadOnlyList<SlotDto>>(url, ct)
               ?? Array.Empty<SlotDto>();
    }

    public async Task<(AppointmentDto? Appointment, string? Error)> BookAsync(
    BookAppointmentRequest request, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("api/appointments", request, ct);
        if (response.IsSuccessStatusCode)
        {
            var appointment = await response.Content
                .ReadFromJsonAsync<AppointmentDto>(cancellationToken: ct);
            return (appointment, null);
        }

        // Try to read the API's ProblemDto; fall back to a generic message.
        try
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDto>(cancellationToken: ct);
            return (null, problem?.Message ?? "Booking failed.");
        }
        catch
        {
            return (null, $"Booking failed ({(int)response.StatusCode}).");
        }
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAppointmentsForCitizenAsync(
        int citizenId, CancellationToken ct) =>
        await _http.GetFromJsonAsync<IReadOnlyList<AppointmentDto>>(
            $"api/appointments/citizen/{citizenId}", ct) ?? Array.Empty<AppointmentDto>();

    public async Task<IReadOnlyList<AccessLogEntryDto>> GetAccessLogAsync(int citizenId, CancellationToken ct) =>
    await _http.GetFromJsonAsync<IReadOnlyList<AccessLogEntryDto>>(
        $"api/access-log/citizen/{citizenId}", ct) ?? Array.Empty<AccessLogEntryDto>();
}