using Vulindlela.Booking.Api.Business;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Booking.Api.Clients;

namespace Vulindlela.Booking.Api.Outbox;

public sealed class OutboxDispatcher : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _log;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcher> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Outbox dispatcher started; polling every {Interval}s.", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Outbox dispatcher iteration failed.");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task ProcessDueAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var repo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var client = scope.ServiceProvider.GetRequiredService<IClearanceClient>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var due = await repo.GetDueAsync(batchSize: 20, clock.UtcNow, ct);
        if (due.Count == 0) return;

        _log.LogInformation("Outbox: {Count} item(s) due.", due.Count);

        foreach (var item in due)
        {
            try
            {
                await client.NotifyAppointmentCreatedAsync(
                    item.AppointmentId, item.CitizenAccountId, item.ServiceType, ct);

                await repo.MarkSentAsync(item.Id, clock.UtcNow, ct);
                _log.LogInformation(
                    "Outbox: delivered appointment {AppointmentId} after {Attempts} attempt(s).",
                    item.AppointmentId, item.AttemptCount + 1);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                // The Clearance service is down or unreachable. Retry with increasing backoff.
                var seconds = Math.Min(MaxBackoff.TotalSeconds,
                    PollInterval.TotalSeconds * Math.Pow(2, item.AttemptCount));
                var next = clock.UtcNow.AddSeconds(seconds);

                await repo.MarkFailedAsync(item.Id, ex.Message, next, ct);

                _log.LogWarning(
                    "Outbox: delivery of appointment {AppointmentId} failed ({Reason}); next attempt at {Next:O}.",
                    item.AppointmentId, ex.GetType().Name, next);
            }
        }
    }
}