namespace NariNoteBackend.Domain.Entity;

/// <summary>
/// Outbox メッセージの送信に失敗した際の、次回試行までの待機時間を決定する。
/// </summary>
public static class OutboxRetryPolicy
{
    /// <summary>
    /// 失敗後、次の試行まで待つ時間を返す。
    /// </summary>
    /// <param name="attempts">これまでの試行回数（初回失敗時は 1）。1 以上 <see cref="OutboxMessage.MaxAttempts"/> 未満。</param>
    public static TimeSpan CalculateDelay(int attempts)
    {
        // TODO: 再試行間隔の方針を決めて実装する（下記は暫定の固定間隔）
        return TimeSpan.FromMinutes(1);
    }
}
