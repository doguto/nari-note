namespace NariNoteBackend.Application.Dto.Response;

public class DeleteArticleForbiddenResponse
{
    public string Message { get; set; } = "この記事を削除する権限がありません";
    public int ArticleId { get; set; }
}
