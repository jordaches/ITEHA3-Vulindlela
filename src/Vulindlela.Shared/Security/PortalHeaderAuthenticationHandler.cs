using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vulindlela.Shared.Security;

public sealed class PortalHeaderAuthenticationHandler : AuthenticationHandler<PortalHeaderAuthenticationOptions>
{
    public PortalHeaderAuthenticationHandler(
        IOptionsMonitor<PortalHeaderAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(PortalHeaderDefaults.InternalKeyHeader, out var keyValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var providedKey = keyValues.ToString();
        if (string.IsNullOrEmpty(providedKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        var expectedBytes = Encoding.UTF8.GetBytes(Options.InternalKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey);

        if (providedBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
        {
            Logger.LogWarning("Portal header auth rejected: internal key mismatch.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal key."));
        }

        // Key is valid. If no citizen headers follow, the caller is the Portal itself.
        if (!Request.Headers.TryGetValue(PortalHeaderDefaults.CitizenIdHeader, out var citizenIdValues) ||
            !int.TryParse(citizenIdValues.ToString(), out var citizenId))
        {
            return Task.FromResult(Success(new[]
            {
                new Claim(ClaimTypes.Role, PortalHeaderDefaults.SystemRole)
            }));
        }

        if (!Request.Headers.TryGetValue(PortalHeaderDefaults.RoleHeader, out var roleValues))
            return Task.FromResult(AuthenticateResult.Fail("Missing role header."));

        var role = roleValues.ToString();
        if (role != PortalHeaderDefaults.CitizenRole && role != PortalHeaderDefaults.OfficialRole)
            return Task.FromResult(AuthenticateResult.Fail("Invalid role header."));

        return Task.FromResult(Success(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, citizenId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }));
    }

    private AuthenticateResult Success(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}