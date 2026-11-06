using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IBookingService _bookings;
        public AppointmentsController(IBookingService bookings) => _bookings = bookings;

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
        public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetForCitizen(
            int citizenAccountId, CancellationToken ct) =>
            Ok(await _bookings.GetForCitizenAsync(citizenAccountId, ct));
    }
}
