namespace NariNoteBackend.Application.Dto.Response;

public class GetArticleNotFoundResponse
{
    public string Message { get; set; } = "記事が見つかりません";
    public int ArticleId { get; set; }
}
