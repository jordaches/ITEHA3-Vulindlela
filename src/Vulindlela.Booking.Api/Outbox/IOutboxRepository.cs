using Vulindlela.Booking.Api.Data.Entities;

namespace Vulindlela.Booking.Api.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyList<NotificationOutbox>> GetDueAsync(
        int batchSize, DateTime nowUtc, CancellationToken ct);

    Task MarkSentAsync(int outboxId, DateTime sentAtUtc, CancellationToken ct);

    Task MarkFailedAsync(
        int outboxId,
        string error,
        DateTime nextAttemptAtUtc,
        CancellationToken ct);
}