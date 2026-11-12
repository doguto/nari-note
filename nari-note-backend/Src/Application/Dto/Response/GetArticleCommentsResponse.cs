namespace NariNoteBackend.Application.Dto.Response;

public class GetArticleCommentsResponse
{
    public List<CommentDto> Comments { get; set; } = new();
    public int TotalCount { get; set; }
}
