namespace NariNoteBackend.Application.Exception;

/// <summary>
/// リソースが見つからない場合の例外（404 Not Found）
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message, System.Exception? innerException = null)
        : base(message, 404, "NOT_FOUND", innerException)
    {
    }
}
