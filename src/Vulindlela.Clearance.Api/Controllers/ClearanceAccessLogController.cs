using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Clearance.Api.Controllers;

[ApiController]
[Route("api/clearance/access-log")]
[Authorize]
public class ClearanceAccessLogController : ControllerBase
{
    private readonly IClearanceAccessLogRepository _logs;
    private readonly IAuthorizationService _authorization;

    public ClearanceAccessLogController(
        IClearanceAccessLogRepository logs,
        IAuthorizationService authorization)
    {
        _logs = logs;
        _authorization = authorization;
    }

    [HttpGet("citizen/{citizenAccountId:int}")]
    public async Task<IActionResult> Get(int citizenAccountId, CancellationToken ct)
    {
        var auth = await _authorization.AuthorizeAsync(
            User, citizenAccountId, PortalHeaderDefaults.CitizenCanAccessOwnRecordPolicy);
        if (!auth.Succeeded) return Forbid();

        var logs = await _logs.GetForCitizenAsync(citizenAccountId, ct);
        return Ok(logs.Select(l => new AccessLogEntryDto(
            "Clearance",
            l.AccessedAtUtc,
            l.AccessedByRole,
            l.AccessedByCitizenId,
            l.Action,
            l.ResourceId)));
    }
}