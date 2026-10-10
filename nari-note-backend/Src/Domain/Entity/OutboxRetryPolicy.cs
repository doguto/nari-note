namespace NariNoteBackend.Domain.Entity;

public static class OutboxRetryPolicy
{
    public static TimeSpan CalculateDelay(int attempts)
    {
        // TODO: 再試行間隔の方針を決めて実装する（暫定の固定間隔）
        return TimeSpan.FromMinutes(1);
    }
}
