namespace NariNoteBackend.Application.Dto.Response;

public class UpdateArticleForbiddenResponse
{
    public string Message { get; set; } = "この記事を更新する権限がありません";
    public int ArticleId { get; set; }
}
