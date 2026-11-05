namespace NariNoteBackend.Application.Exception;

// 認証済みだが対象リソースへの操作権限が無い場合の例外（403 Forbidden）
public class ForbiddenException : System.Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
