namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 短時間に繰り返された操作を拒否する場合の例外（429 Too Many Requests）
/// </summary>
public class TooManyRequestsException : System.Exception
{
    public TooManyRequestsException(string message) : base(message)
    {
    }
}
