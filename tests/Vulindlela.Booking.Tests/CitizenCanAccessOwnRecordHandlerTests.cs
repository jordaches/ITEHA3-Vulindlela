using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Vulindlela.Shared.Security;

namespace Vulindlela.Booking.Tests;

public class CitizenCanAccessOwnRecordHandlerTests
{
    private static async Task<bool> EvaluateAsync(
        string role, int? callerCitizenId, int resourceCitizenId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (callerCitizenId is int id)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var requirement = new CitizenCanAccessOwnRecordRequirement();
        var handler = new CitizenCanAccessOwnRecordHandler();

        var context = new AuthorizationHandlerContext(
            new[] { requirement }, user, resource: resourceCitizenId);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Citizen_can_read_their_own_record()
        => Assert.True(await EvaluateAsync("Citizen", callerCitizenId: 42, resourceCitizenId: 42));

    [Fact]
    public async Task Citizen_cannot_read_another_citizens_record()
        => Assert.False(await EvaluateAsync("Citizen", callerCitizenId: 42, resourceCitizenId: 43));

    [Fact]
    public async Task Official_can_read_any_record()
        => Assert.True(await EvaluateAsync("Official", callerCitizenId: 1, resourceCitizenId: 999));

    [Fact]
    public async Task System_principal_can_read_nothing()
        => Assert.False(await EvaluateAsync("System", callerCitizenId: null, resourceCitizenId: 1));
}