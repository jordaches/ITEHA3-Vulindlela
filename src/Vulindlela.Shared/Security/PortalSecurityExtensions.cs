using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vulindlela.Shared.Security;

public static class PortalSecurityExtensions
{

    /// Registers the PortalHeader authentication scheme and the
    /// CitizenCanAccessOwnRecord policy. Reads Security:InternalKey from
    /// configuration and fails fast if it is missing.

    public static IServiceCollection AddPortalHeaderSecurity(
        this IServiceCollection services, IConfiguration configuration)
    {
        var internalKey = configuration["Security:InternalKey"];
        if (string.IsNullOrWhiteSpace(internalKey))
        {
            throw new InvalidOperationException(
                "Security:InternalKey is not configured. " +
                "Set the VULINDLELA_INTERNAL_KEY environment variable, or in Development " +
                "add Security:InternalKey to appsettings.Development.json. See docs/dev-setup.md.");
        }

        services
            .AddAuthentication(PortalHeaderDefaults.Scheme)
            .AddScheme<PortalHeaderAuthenticationOptions, PortalHeaderAuthenticationHandler>(
                PortalHeaderDefaults.Scheme,
                options => options.InternalKey = internalKey);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PortalHeaderDefaults.CitizenCanAccessOwnRecordPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new CitizenCanAccessOwnRecordRequirement());
            });
        });

        services.AddSingleton<IAuthorizationHandler, CitizenCanAccessOwnRecordHandler>();

        return services;
    }
}