namespace NariNoteBackend.Infrastructure.Cleanup;

public class UnverifiedUserCleanupWorker : BackgroundService
{
    static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    readonly ILogger<UnverifiedUserCleanupWorker> logger;
    readonly IServiceScopeFactory scopeFactory;

    public UnverifiedUserCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<UnverifiedUserCleanupWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await ProcessAsync();
        } while (await WaitNextTickAsync(timer, stoppingToken));
    }

    async Task ProcessAsync()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<UnverifiedUserCleanupProcessor>().ProcessAsync();
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Unverified user cleanup failed");
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
