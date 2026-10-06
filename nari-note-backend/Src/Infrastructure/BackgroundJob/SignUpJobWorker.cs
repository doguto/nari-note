using NariNoteBackend.Application.BackgroundJob;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.BackgroundJob;

/// <summary>
/// サインアップジョブを1件ずつ順番に処理するワーカー
/// 単一のワーカーで直列に処理するため、同一メールアドレスの同時申し込みでも重複登録は起きない
/// </summary>
public class SignUpJobWorker : BackgroundService
{
    readonly ISignUpJobQueue signUpJobQueue;
    readonly IServiceScopeFactory serviceScopeFactory;
    readonly ILogger<SignUpJobWorker> logger;

    public SignUpJobWorker(
        ISignUpJobQueue signUpJobQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SignUpJobWorker> logger
    )
    {
        this.signUpJobQueue = signUpJobQueue;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SignUpJob job;
            try
            {
                job = await this.signUpJobQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ProcessAsync(job);
            }
            catch (Exception ex)
            {
                // レスポンスは返却済みのため、失敗はログに残して次のジョブへ進む
                this.logger.LogError(ex, "サインアップジョブの処理に失敗しました");
            }
        }
    }

    async Task ProcessAsync(SignUpJob job)
    {
        using var scope = this.serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
        var handler = scope.ServiceProvider.GetRequiredService<SignUpJobHandler>();

        // TransactionMiddleware の外で実行されるため、ここでトランザクションを張る
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await handler.ExecuteAsync(job);
        await transaction.CommitAsync();
    }
}
