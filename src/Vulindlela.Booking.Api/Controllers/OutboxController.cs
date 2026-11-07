using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;

namespace Vulindlela.Booking.Api.Controllers;

[Authorize(Roles = "System")]
[ApiController]
[Route("api/outbox")]
public class OutboxController : ControllerBase
{
    private readonly BookingDbContext _db;
    public OutboxController(BookingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetRecent(CancellationToken ct) =>
        Ok(await _db.Outbox
                     .OrderByDescending(o => o.CreatedAtUtc)
                     .Take(20)
                     .Select(o => new
                     {
                         o.Id,
                         o.AppointmentId,
                         o.ServiceType,
                         o.CreatedAtUtc,
                         o.SentAtUtc,
                         o.AttemptCount,
                         o.NextAttemptAtUtc,
                         o.LastError
                     })
                     .ToListAsync(ct));
}