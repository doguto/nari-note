namespace NariNoteBackend.Application.Dto.Request;

public class DeleteArticleRequest
{
    public int Id { get; set; }
    public int UserId { get; set; } // 認証実装後は削除
}
