namespace NariNoteBackend.Application.BackgroundJob;

/// <summary>
/// サインアップ要求をバックグラウンドで処理するためのジョブ
/// 平文パスワードを保持しないよう、ハッシュ化済みのパスワードを受け渡す
/// </summary>
public record SignUpJob(string Name, string Email, string PasswordHash);
