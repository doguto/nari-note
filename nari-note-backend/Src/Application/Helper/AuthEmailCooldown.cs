namespace NariNoteBackend.Application.Helper;

// 認証まわりのメール（確認メール・登録済みの案内・パスワード再設定）を同じアドレスへ連続して送らないための送信間隔。
// 他人のメールアドレスを指定した API の連打で、そのアドレスへ大量のメールが届くのを防ぐ。
public static class AuthEmailCooldown
{
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(60);

    public static bool IsActive(DateTime? lastSentAt, DateTime now)
    {
        return lastSentAt != null && now - lastSentAt.Value < Duration;
    }
}
