namespace NariNoteBackend.Application.Dto.Response;

public class ToggleLikeResponse
{
    public bool IsLiked { get; set; }
    public int ArticleId { get; set; }
    public int UserId { get; set; }
    public int CurrentLikeCount { get; set; }
}
