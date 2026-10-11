namespace NariNoteBackend.Infrastructure.Outbox;

public class OutboxWorker : BackgroundService
{
    static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    readonly ILogger<OutboxWorker> logger;
    readonly IServiceScopeFactory scopeFactory;

    public OutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        do
        {
            await ProcessUntilDrainedAsync(stoppingToken);
        } while (await WaitNextTickAsync(timer, stoppingToken));
    }

    async Task ProcessUntilDrainedAsync(CancellationToken stoppingToken)
    {
        try
        {
            int processed;
            do
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                processed = await processor.ProcessBatchAsync();
            } while (processed >= OutboxProcessor.BatchSize && !stoppingToken.IsCancellationRequested);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Outbox polling failed");
        }
    }

    static async Task<bool> WaitNextTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
