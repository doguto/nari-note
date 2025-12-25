namespace NariNoteBackend.Application.Dto.Response;

public class CreateArticleBadRequestResponse
{
    public string Message { get; set; } = "入力値が不正です";
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}
