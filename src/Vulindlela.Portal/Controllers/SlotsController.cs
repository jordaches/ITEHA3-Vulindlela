using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Clients;
using Vulindlela.Portal.ViewModels;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Portal.Controllers;

[Authorize]
public class SlotsController : Controller
{
    private readonly IBookingApiClient _booking;
    public SlotsController(IBookingApiClient booking) => _booking = booking;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            return View(await _booking.GetBranchesAsync(ct));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ViewData["BookingDown"] = true;
            return View(new List<BranchDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Browse(
        int branchId, string date, string? serviceType, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            var d = DateTime.UtcNow.Date.AddDays(7);
            while (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) d = d.AddDays(1);
            date = d.ToString("yyyy-MM-dd");
        }

        var branches = await _booking.GetBranchesAsync(ct);
        var branch = branches.FirstOrDefault(b => b.Id == branchId);
        if (branch is null) return NotFound();

        var slots = await _booking.GetSlotsAsync(branchId, date, serviceType, ct);
        return View(new BrowseSlotsViewModel(branchId, branch.Name, date, serviceType, slots));
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(
    int slotId,
    string serviceType,
    int branchId,
    string date,
    CancellationToken ct)
    {
        var citizenId = User.GetCitizenId();
        if (citizenId is null) return Challenge();

        var (appointment, error) = await _booking.BookAsync(
            new BookAppointmentRequest(slotId, citizenId.Value, serviceType), ct);

        if (appointment is null)
        {
            TempData["Error"] = error ?? "Booking failed.";
            return RedirectToAction(nameof(Browse), new { branchId, date, serviceType });
        }

        TempData["Success"] = $"Booked. Reference {appointment.Reference}.";
        return RedirectToAction("Mine", "Appointments");
    }
}