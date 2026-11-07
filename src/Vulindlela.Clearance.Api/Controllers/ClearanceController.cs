using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Clearance.Api.Business;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Clearance.Api.Controllers;

[ApiController]
[Route("api/clearance")]
public class ClearanceController : ControllerBase
{
    private readonly ISubmissionService _submissions;

    private readonly IAuthorizationService _authorization;
    private readonly IClearanceAccessLogRepository _logs;

    public ClearanceController(ISubmissionService submissions,IAuthorizationService authorization, IClearanceAccessLogRepository logs)
    {
        _submissions = submissions;
        _authorization = authorization;
        _logs = logs;
    }

    /// Booking calls this when an appointment is created.
    [Authorize(Roles = "System")] 
    [HttpPost("appointments")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterAppointmentRequest req, CancellationToken ct)
    {
        var dto = await _submissions.RegisterAppointmentAsync(req, ct);
        return dto is null
            ? BadRequest(new ProblemDto("bad_request", "Invalid request."))
            : Ok(dto);
    }

    [Authorize]
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

    [Authorize]                        // resource-based check below
    [HttpGet("appointments/{appointmentId:int}")]
    public async Task<IActionResult> GetStatus(int appointmentId, CancellationToken ct)
    {
        var dto = await _submissions.GetStatusAsync(appointmentId, ct);
        if (dto is null)
            return NotFound(new ProblemDto("not_found", "No clearance record for that appointment."));

        // The route carries the appointmentId; the resource for authorisation is the citizenAccountId in the status row when Booking is notified.
        var record = await _submissions.GetStatusForAuthorisationAsync(appointmentId, ct);
        if (record is null) return NotFound();

        var auth = await _authorization.AuthorizeAsync(
            User, record.Value.CitizenAccountId, PortalHeaderDefaults.CitizenCanAccessOwnRecordPolicy);
        if (!auth.Succeeded) return Forbid();

        var log = new Vulindlela.Clearance.Api.Data.Entities.ClearanceAccessLog
        {
            CitizenAccountId = record.Value.CitizenAccountId,
            AccessedByCitizenId = User.GetCitizenId(),
            AccessedByRole = User.GetRole() ?? "System",
            AccessedByName = User.Identity?.Name ?? "System",
            AccessedAtUtc = DateTime.UtcNow,
            Action = "View Clearance Status",
            ResourceId = appointmentId.ToString()
        };
        await _logs.AddAsync(log, ct);

        return Ok(dto);
    }
}