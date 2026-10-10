namespace NariNoteBackend.Infrastructure.Outbox;

/// <summary>
/// Outbox を一定間隔でポーリングし、未送信メッセージを送信する常駐ワーカー。
/// BackgroundService は Singleton のため、Scoped な依存は処理ごとにスコープを作って解決する。
/// </summary>
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

    // バッチが満杯の間は待機せず続けて処理する
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
            // 停止要求
        }
        catch (System.Exception ex)
        {
            // DB 障害などでループが終了しないよう、次の周期で再試行する
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
