using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Infrastructure.Cleanup;

/// <summary>
///     メールアドレスの認証が完了しないまま放置されたユーザーを削除する。
///     他人のメールアドレスや存在しないメールアドレスで登録されたユーザーが、名前・メールアドレスを使えない状態のまま残らないようにする。
/// </summary>
public class UnverifiedUserCleanupProcessor
{
    // 作成からこの期間が経過しても認証されていないユーザーを削除対象にする
    public static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    readonly ILogger<UnverifiedUserCleanupProcessor> logger;
    readonly TimeProvider timeProvider;
    readonly IUserRepository userRepository;

    public UnverifiedUserCleanupProcessor(
        IUserRepository userRepository,
        ILogger<UnverifiedUserCleanupProcessor> logger,
        TimeProvider timeProvider
    )
    {
        this.userRepository = userRepository;
        this.logger = logger;
        this.timeProvider = timeProvider;
    }

    public async Task<int> ProcessAsync()
    {
        var now = timeProvider.UtcNow();
        // 有効な確認トークンが残っているユーザーは、作成から期間が経過していても削除しない（再度サインアップした直後など）
        var deleted = await userRepository.DeleteStaleUnverifiedAsync(now - Retention, now);
        if (deleted > 0) logger.LogInformation("Unverified users deleted. Count={Count}", deleted);

        return deleted;
    }
}
