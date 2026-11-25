namespace NariNoteBackend.Infrastructure.Job;

public class ShapeJudgeWorker : BackgroundService
{
    static readonly TimeSpan IdleInterval = TimeSpan.FromSeconds(5);
    // エンジンサービスが落ちているときにログを埋めないよう、失敗後は長めに待つ
    static readonly TimeSpan ErrorInterval = TimeSpan.FromMinutes(1);

    readonly ILogger<ShapeJudgeWorker> logger;
    readonly IServiceScopeFactory scopeFactory;

    public ShapeJudgeWorker(IServiceScopeFactory scopeFactory, ILogger<ShapeJudgeWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await this.ResetAsync()) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            var wait = IdleInterval;
            try
            {
                using var scope = this.scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ShapeJudgeProcessor>();
                if (await processor.ProcessAsync(stoppingToken)) wait = TimeSpan.Zero;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, "Shape judge processing failed");
                wait = ErrorInterval;
            }

            if (wait == TimeSpan.Zero) continue;
            try
            {
                await Task.Delay(wait, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    // 前回の停止時に判定中だった候補を待ち行列に戻す。エンジンが無い環境では何もしない
    async Task<bool> ResetAsync()
    {
        try
        {
            using var scope = this.scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<ShapeJudgeProcessor>();
            if (!processor.IsEnabled) return false;
            await processor.ResetAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            this.logger.LogError(ex, "Shape judge reset failed");
            return true;
        }
    }
}
