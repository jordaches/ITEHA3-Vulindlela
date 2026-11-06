using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Outbox;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly BookingDbContext _db;
    public OutboxRepository(BookingDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationOutbox>> GetDueAsync(
        int batchSize, DateTime nowUtc, CancellationToken ct) =>
        await _db.Outbox
                 .Where(o => o.SentAtUtc == null
                          && (o.NextAttemptAtUtc == null || o.NextAttemptAtUtc <= nowUtc))
                 .OrderBy(o => o.NextAttemptAtUtc ?? o.CreatedAtUtc)
                 .Take(batchSize)
                 .ToListAsync(ct);

    public async Task MarkSentAsync(int outboxId, DateTime sentAtUtc, CancellationToken ct)
    {
        var row = await _db.Outbox.FirstOrDefaultAsync(o => o.Id == outboxId, ct);
        if (row is null) return;

        row.SentAtUtc = sentAtUtc;
        row.AttemptCount++;
        row.LastError = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(
        int outboxId, string error, DateTime nextAttemptAtUtc, CancellationToken ct)
    {
        var row = await _db.Outbox.FirstOrDefaultAsync(o => o.Id == outboxId, ct);
        if (row is null) return;

        row.AttemptCount++;
        row.NextAttemptAtUtc = nextAttemptAtUtc;
        row.LastError = error.Length > 1000 ? error[..1000] : error;
        await _db.SaveChangesAsync(ct);
    }
}