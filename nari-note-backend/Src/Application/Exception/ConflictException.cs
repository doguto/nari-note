namespace NariNoteBackend.Application.Exception;

/// <summary>
/// リソースの競合が発生した場合の例外（409 Conflict）
/// 例：既に存在するデータの作成を試みた場合
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message, System.Exception? innerException = null)
        : base(message, 409, "CONFLICT", innerException)
    {
    }
}
