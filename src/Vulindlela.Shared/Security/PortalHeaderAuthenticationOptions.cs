using Microsoft.AspNetCore.Authentication;

namespace Vulindlela.Shared.Security;

public sealed class PortalHeaderAuthenticationOptions : AuthenticationSchemeOptions
{
    public string InternalKey { get; set; } = string.Empty;
}