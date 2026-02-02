using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetCourseContentResponse
{
    public CourseId Id { get; set; }
    public required string Name { get; set; }
    public UserId UserId { get; set; }
    public required string UserName { get; set; }
    public int LikeCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CourseArticleDto> Articles { get; set; } = new();
}

public class CourseArticleDto
{
    public ArticleId Id { get; set; }
    public required string Title { get; set; }
    public int? ArticleOrder { get; set; }
    public bool IsPublished { get; set; }
}
