using System.Security.Claims;

namespace Vulindlela.Shared.Security;

public static class ClaimsPrincipalExtensions
{
    public static int? GetCitizenId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    public static string? GetRole(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Role)?.Value;
}