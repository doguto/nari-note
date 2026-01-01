namespace NariNoteBackend.Application.Dto.Response;

public class ToggleLikeNotFoundResponse
{
    public string Message { get; set; } = "記事が見つかりません";
    public int ArticleId { get; set; }
}
