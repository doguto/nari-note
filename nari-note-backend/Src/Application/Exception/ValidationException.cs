namespace NariNoteBackend.Application.Exception;

/// <summary>
/// 入力値が不正な場合の例外（400 Bad Request）
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException(
        string message,
        Dictionary<string, object>? validationErrors = null,
        System.Exception? innerException = null)
        : base(message, 400, "VALIDATION_ERROR", innerException, validationErrors)
    {
    }
}
