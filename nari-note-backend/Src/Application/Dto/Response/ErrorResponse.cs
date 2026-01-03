namespace NariNoteBackend.Application.Dto.Response;

public class ErrorResponse
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
