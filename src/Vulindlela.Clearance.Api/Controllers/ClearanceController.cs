using Microsoft.AspNetCore.Mvc;
using Vulindlela.Clearance.Api.Business;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Clearance.Api.Controllers;

[ApiController]
[Route("api/clearance")]
public class ClearanceController : ControllerBase
{
    private readonly ISubmissionService _submissions;
    public ClearanceController(ISubmissionService submissions) => _submissions = submissions;

    /// <summary>Booking calls this when an appointment is created (Phase C wires it up).</summary>
    [HttpPost("appointments")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterAppointmentRequest req, CancellationToken ct)
    {
        var dto = await _submissions.RegisterAppointmentAsync(req, ct);
        return dto is null
            ? BadRequest(new ProblemDto("bad_request", "Invalid request."))
            : Ok(dto);
    }

    [HttpPost("submissions")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitDocumentsRequest req, CancellationToken ct)
    {
        var dto = await _submissions.SubmitAsync(req, ct);
        if (dto is null)
            return Conflict(new ProblemDto(
                "not_acceptable",
                "Submission cannot be accepted in the current state."));
        return Accepted(dto);   // 202 — the worker does the work
    }

    [HttpGet("appointments/{appointmentId:int}")]
    public async Task<IActionResult> GetStatus(int appointmentId, CancellationToken ct)
    {
        var dto = await _submissions.GetStatusAsync(appointmentId, ct);
        return dto is null
            ? NotFound(new ProblemDto("not_found", "No clearance record for that appointment."))
            : Ok(dto);
    }
}