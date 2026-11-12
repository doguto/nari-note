using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticleContentResponse
{
    public required ArticleDto Article { get; set; }
    public bool IsLiked { get; set; }
    public CourseId? CourseId { get; set; }
    public string? CourseName { get; set; }
}
