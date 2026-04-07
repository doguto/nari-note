using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class CourseArticleDto
{
    public ArticleId Id { get; set; }
    public required string Title { get; set; }
    public int? ArticleOrder { get; set; }
    public bool IsPublished { get; set; }
}
