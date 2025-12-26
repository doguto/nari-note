namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 認証が必要な場合の例外（401 Unauthorized）
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message, System.Exception? innerException = null)
        : base(message, 401, "UNAUTHORIZED", innerException)
    {
    }
}
