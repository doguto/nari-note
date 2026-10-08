namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 認証済みだが対象リソースへの操作権限が無い場合の例外（403 Forbidden）
/// </summary>
public class ForbiddenException : System.Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
