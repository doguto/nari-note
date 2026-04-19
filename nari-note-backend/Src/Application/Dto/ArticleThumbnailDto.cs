using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class ArticleThumbnailDto
{
    public ArticleId Id { get; set; }
    public required string Title { get; set; }
    public UserId AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public string? UserIconImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public int LikeCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
