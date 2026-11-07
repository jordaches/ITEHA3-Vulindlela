using Microsoft.AspNetCore.Authentication.Cookies;
using Vulindlela.Portal.Clients;
using Vulindlela.Portal.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:9001");

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Cookie auth for the citizen
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/SignOut";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Identity forwarding handler: adds X-Internal-Key always, plus X-Citizen-Id and
// X-Citizen-Role when the Portal's HttpContext has an authenticated user.
builder.Services.AddTransient<IdentityForwardingHandler>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    var config = sp.GetRequiredService<IConfiguration>();
    var key = config["Security:InternalKey"]
        ?? throw new InvalidOperationException(
            "Security:InternalKey is not configured. See docs/dev-setup.md.");
    return new IdentityForwardingHandler(accessor, key);
});

builder.Services.AddHttpClient<IBookingApiClient, BookingApiClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Services:BookingApi"]!);
    http.Timeout = TimeSpan.FromSeconds(5);
}).AddHttpMessageHandler<IdentityForwardingHandler>();

builder.Services.AddHttpClient<IClearanceApiClient, ClearanceApiClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Services:ClearanceApi"]!);
    http.Timeout = TimeSpan.FromSeconds(2);   // short so degradation surfaces quickly
}).AddHttpMessageHandler<IdentityForwardingHandler>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }