namespace NariNoteBackend.Application.Dto.Request;

public class ToggleLikeRequest
{
    public int ArticleId { get; set; }
    public int UserId { get; set; }
}
