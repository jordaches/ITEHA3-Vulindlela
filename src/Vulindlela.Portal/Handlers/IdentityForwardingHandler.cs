using Vulindlela.Shared.Security;

namespace Vulindlela.Portal.Handlers;

public sealed class IdentityForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    private readonly string _internalKey;

    public IdentityForwardingHandler(IHttpContextAccessor accessor, string internalKey)
    {
        _accessor = accessor;
        _internalKey = internalKey;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(PortalHeaderDefaults.InternalKeyHeader, _internalKey);

        var user = _accessor.HttpContext?.User;
        if (user is { Identity.IsAuthenticated: true })
        {
            var id = user.GetCitizenId();
            if (id.HasValue)
                request.Headers.TryAddWithoutValidation(
                    PortalHeaderDefaults.CitizenIdHeader, id.Value.ToString());

            var role = user.GetRole();
            if (!string.IsNullOrEmpty(role))
                request.Headers.TryAddWithoutValidation(PortalHeaderDefaults.RoleHeader, role);
        }

        return base.SendAsync(request, cancellationToken);
    }
}