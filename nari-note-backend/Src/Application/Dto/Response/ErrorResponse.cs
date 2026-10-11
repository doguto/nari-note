namespace NariNoteBackend.Application.Dto.Response;

public class ErrorResponse
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public required DateTime TimeStamp { get; set; }

    // クライアントがエラー種別を判別する必要がある場合のみ設定する
    public string? Code { get; set; }
}
