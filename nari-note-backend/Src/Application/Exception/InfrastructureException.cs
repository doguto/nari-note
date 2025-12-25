namespace NariNoteBackend.Application.Exception;

/// <summary>
/// インフラストラクチャ層でのエラー（500 Internal Server Error）
/// 例：データベースエラー、外部APIエラー
/// </summary>
public class InfrastructureException : ApplicationException
{
    public InfrastructureException(string message, System.Exception? innerException = null)
        : base(message, 500, "INFRASTRUCTURE_ERROR", innerException)
    {
    }
}
