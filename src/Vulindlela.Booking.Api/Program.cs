using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Business.Security;
using Vulindlela.Booking.Api.Clients;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Data.Repositories;
using Vulindlela.Booking.Api.Data.Seed;
using Vulindlela.Booking.Api.Outbox;
using Vulindlela.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:9002");

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BookingDb")));

// Data access
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICitizenAccountRepository, CitizenAccountRepository>();

// Business
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IFeeCalculator, FeeCalculator>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IAccessLogRepository, AccessLogRepository>();

builder.Services.AddHttpClient<IClearanceClient, ClearanceClient>(http =>
{
    http.BaseAddress = new Uri(
        builder.Configuration["Services:ClearanceApi"]
        ?? throw new InvalidOperationException("Services:ClearanceApi is not configured."));
    http.Timeout = TimeSpan.FromSeconds(2);

    var internalKey = builder.Configuration["Security:InternalKey"]
        ?? throw new InvalidOperationException(
            "Security:InternalKey is not configured. See docs/dev-setup.md.");

    http.DefaultRequestHeaders.TryAddWithoutValidation("X-Internal-Key", internalKey);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddPortalHeaderSecurity(builder.Configuration);

// Outbox
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    db.Database.EnsureCreated();
    BookingSeeder.Seed(db, hasher);
}

app.MapControllers();
app.Run();

public partial class Program { }