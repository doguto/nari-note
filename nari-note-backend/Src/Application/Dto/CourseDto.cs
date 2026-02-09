using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class CourseDto
{
    public CourseId Id { get; set; }
    public UserId UserId { get; set; }
    public string? UserName { get; set; }
    public required string Name { get; set; }
    public List<ArticleId> ArticleIds { get; set; } = new();
    public List<string> ArticleNames { get; set; } = new();
    public int LikeCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
