namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 既存のリソースと競合する場合の例外（409 Conflict）
/// </summary>
public class ConflictException : System.Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
