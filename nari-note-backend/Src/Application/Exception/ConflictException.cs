namespace NariNoteBackend.Application.Exception;

// 既存のリソースと競合する場合の例外（409 Conflict）
public class ConflictException : System.Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
