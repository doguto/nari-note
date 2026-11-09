namespace NariNoteBackend.Infrastructure.Job;

public class UnverifiedUserDeletionWorker : BackgroundService
{
    static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    readonly ILogger<UnverifiedUserDeletionWorker> logger;
    readonly IServiceScopeFactory scopeFactory;

    public UnverifiedUserDeletionWorker(IServiceScopeFactory scopeFactory, ILogger<UnverifiedUserDeletionWorker> logger)
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
            await scope.ServiceProvider.GetRequiredService<UnverifiedUserDeletionProcessor>().ProcessAsync();
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Unverified user deletion failed");
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
