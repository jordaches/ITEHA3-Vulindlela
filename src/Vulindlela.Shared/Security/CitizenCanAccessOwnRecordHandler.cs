using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Vulindlela.Shared.Security;


public sealed class CitizenCanAccessOwnRecordHandler
    : AuthorizationHandler<CitizenCanAccessOwnRecordRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CitizenCanAccessOwnRecordRequirement requirement,
        int resourceCitizenId)
    {
        if (context.User.IsInRole(PortalHeaderDefaults.OfficialRole))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is not null
            && int.TryParse(claim.Value, out var callerCitizenId)
            && callerCitizenId == resourceCitizenId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}