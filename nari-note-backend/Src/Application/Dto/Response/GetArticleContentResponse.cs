using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticleContentResponse
{
    public ArticleId Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public UserId AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public List<string> Tags { get; set; } = new();
    public int LikeCount { get; set; }
    public bool IsLiked { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public CourseId? CourseId { get; set; }
    public string? CourseName { get; set; }
}
