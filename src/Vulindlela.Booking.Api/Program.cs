using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Clients;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Data.Repositories;
using Vulindlela.Booking.Api.Data.Seed;
using Vulindlela.Booking.Api.Outbox;

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

builder.Services.AddHttpClient<IClearanceClient, ClearanceClient>(http =>
{
    http.BaseAddress = new Uri(
        builder.Configuration["Services:ClearanceApi"]
        ?? throw new InvalidOperationException("Services:ClearanceApi is not configured."));
    http.Timeout = TimeSpan.FromSeconds(2);
});

// Outbox
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();     // builds the schema including the unique index
    BookingSeeder.Seed(db);
}

app.MapControllers();
app.Run();

public partial class Program { }   // enables WebApplicationFactory in later weeks