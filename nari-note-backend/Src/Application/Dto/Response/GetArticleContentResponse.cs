using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticleContentResponse
{
    public ArticleDto Article { get; set; }
    public bool IsLiked { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public CourseId? CourseId { get; set; }
    public string? CourseName { get; set; }
}
