using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Clients;
using Vulindlela.Portal.ViewModels;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Portal.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly IBookingApiClient _booking;
    private readonly IClearanceApiClient _clearance;

    public AppointmentsController(IBookingApiClient booking, IClearanceApiClient clearance)
    {
        _booking = booking;
        _clearance = clearance;
    }

    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var id = User.GetCitizenId();
        if (id is null) return Challenge();

        IReadOnlyList<AppointmentDto> appointments = Array.Empty<AppointmentDto>();
        string? bookingError = null;
        try
        {
            appointments = await _booking.GetAppointmentsForCitizenAsync(id.Value, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            bookingError = "Your appointments are temporarily unavailable.";
        }

        return await ComposeAsync(
            User.Identity?.Name ?? "You", appointments, bookingError, ct);
    }

    [HttpGet]
    public async Task<IActionResult> ForCitizen(int id, CancellationToken ct)
    {
        if (!User.IsInRole(PortalHeaderDefaults.OfficialRole)) return Forbid();

        IReadOnlyList<AppointmentDto> appointments = Array.Empty<AppointmentDto>();
        string? bookingError = null;
        try
        {
            appointments = await _booking.GetAppointmentsForCitizenAsync(id, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            bookingError = "Citizen appointments are temporarily unavailable.";
        }

        return await ComposeAsync($"Citizen #{id}", appointments, bookingError, ct);
    }

    private async Task<IActionResult> ComposeAsync(
        string ownerName,
        IReadOnlyList<AppointmentDto> appointments,
        string? bookingError,
        CancellationToken ct)
    {
        IReadOnlyDictionary<int, ClearanceStatusDto> clearance =
            new Dictionary<int, ClearanceStatusDto>();
        string? clearanceError = null;

        if (appointments.Count > 0)
        {
            try
            {
                clearance = await _clearance.GetStatusesAsync(
                    appointments.Select(a => a.Id).ToList(), ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                clearanceError = "Clearance status is temporarily unavailable.";
            }
        }

        return View("Mine", new MyAppointmentsViewModel(
            ownerName, appointments, clearance, clearanceError, bookingError));
    }
}