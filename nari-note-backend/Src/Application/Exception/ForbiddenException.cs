namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 権限が不足している場合の例外（403 Forbidden）
/// </summary>
public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message, System.Exception? innerException = null)
        : base(message, 403, "FORBIDDEN", innerException)
    {
    }
}
