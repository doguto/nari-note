namespace NariNoteBackend.Application.Dto.Response;

/// <summary>
/// エラーレスポンスの外側のラッパー
/// </summary>
public class ErrorResponse
{
    public ErrorDetail Error { get; set; } = default!;
}

/// <summary>
/// エラー詳細情報
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// エラーコード（例：NOT_FOUND, VALIDATION_ERROR）
    /// </summary>
    public string Code { get; set; } = default!;
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = default!;
    
    /// <summary>
    /// エラー発生時刻（UTC）
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// リクエストパス
    /// </summary>
    public string Path { get; set; } = default!;
    
    /// <summary>
    /// 追加のエラー情報（任意）
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
