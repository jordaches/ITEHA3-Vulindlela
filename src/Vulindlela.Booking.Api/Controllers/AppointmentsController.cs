using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IBookingService _bookings;
    private readonly IAuthorizationService _authorization;

    private readonly IAccessLogRepository _logs;

    public AppointmentsController(
        IBookingService bookings,
        IAuthorizationService authorization,
        IAccessLogRepository logs)
    {
        _bookings = bookings;
        _authorization = authorization;
        _logs = logs;
    }

    [HttpPost]
    public async Task<IActionResult> Book(
        [FromBody] BookAppointmentRequest request, CancellationToken ct)
    {
        var result = await _bookings.BookAsync(request, ct);
        return result.Outcome switch
        {
            BookingOutcome.Success =>
                CreatedAtAction(nameof(GetById), new { id = result.Appointment!.Id }, result.Appointment),
            BookingOutcome.NotFound =>
                NotFound(new ProblemDto("not_found", result.Message!)),
            BookingOutcome.Rejected =>
                BadRequest(new ProblemDto("rejected", result.Message!)),
            BookingOutcome.Conflict =>
                Conflict(new ProblemDto("slot_taken", result.Message!)),
            _ => StatusCode(500, new ProblemDto("error", "Unexpected outcome."))
        };
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var appointment = await _bookings.GetAsync(id, ct);
        return appointment is null
            ? NotFound(new ProblemDto("not_found", "Appointment does not exist."))
            : Ok(appointment);
    }

    [HttpGet("citizen/{citizenAccountId:int}")]
    public async Task<IActionResult> GetForCitizen(int citizenAccountId, CancellationToken ct)
    {
        var auth = await _authorization.AuthorizeAsync(
            User, citizenAccountId, PortalHeaderDefaults.CitizenCanAccessOwnRecordPolicy);
        if (!auth.Succeeded) return Forbid();

        var log = new Vulindlela.Booking.Api.Data.Entities.BookingAccessLog
        {
            CitizenAccountId = citizenAccountId,
            AccessedByCitizenId = User.GetCitizenId(),
            AccessedByRole = User.GetRole() ?? "System",
            AccessedAtUtc = DateTime.UtcNow,
            Action = "View Appointments"
        };
        await _logs.AddAsync(log, ct);

        return Ok(await _bookings.GetForCitizenAsync(citizenAccountId, ct));
    }
}