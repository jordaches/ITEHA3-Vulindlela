using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Business.Queue;

namespace Vulindlela.Clearance.Api.Business.Workers;

public sealed class ClearanceWorker : BackgroundService
{
    private readonly IDocumentQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClearanceWorker> _log;

    public ClearanceWorker(
        IDocumentQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ClearanceWorker> log)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Clearance worker started.");
        await foreach (var submissionId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var verifier = scope.ServiceProvider.GetRequiredService<IVerificationService>();
                await verifier.VerifyAsync(submissionId, stoppingToken);
                _log.LogInformation("Verified submission {Id}.", submissionId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Verification failed for submission {Id}.", submissionId);
            }
        }
    }
}