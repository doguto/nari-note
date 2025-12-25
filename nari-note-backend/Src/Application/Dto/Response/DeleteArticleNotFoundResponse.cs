namespace NariNoteBackend.Application.Dto.Response;

public class DeleteArticleNotFoundResponse
{
    public string Message { get; set; } = "記事が見つかりません";
    public int ArticleId { get; set; }
}
