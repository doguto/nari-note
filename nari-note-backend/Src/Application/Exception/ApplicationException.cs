namespace NariNoteBackend.Application.Exception;

/// <summary>
/// アプリケーション例外の基底クラス
/// すべてのカスタム例外はこのクラスを継承する
/// </summary>
public abstract class ApplicationException : System.Exception
{
    /// <summary>
    /// HTTPステータスコード
    /// </summary>
    public int StatusCode { get; }
    
    /// <summary>
    /// エラーコード（例：NOT_FOUND, VALIDATION_ERROR）
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// 追加のエラー情報（任意）
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; }

    protected ApplicationException(
        string message,
        int statusCode,
        string errorCode,
        System.Exception? innerException = null,
        Dictionary<string, object>? additionalData = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        AdditionalData = additionalData;
    }
}
