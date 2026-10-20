using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetCourseContentResponse
{
    public CourseId Id { get; set; }
    public required string Name { get; set; }
    public UserId UserId { get; set; }
    public required string UserName { get; set; }
    public string? UserIconImageUrl { get; set; }
    public int LikeCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CourseArticleDto> Articles { get; set; } = new();

    // 閲覧用・編集用の取得 API で共用する
    public static GetCourseContentResponse From(Course course) => new()
    {
        Id = course.Id,
        Name = course.Name,
        UserId = course.UserId,
        UserName = course.User?.Name ?? "",
        UserIconImageUrl = course.User?.ProfileImage,
        LikeCount = course.LikeCount,
        IsPublished = course.IsPublished,
        PublishedAt = course.PublishedAt,
        CreatedAt = course.CreatedAt,
        Articles = course.Articles
            .OrderBy(a => a.ArticleOrder)
            .Select(a => new CourseArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                ArticleOrder = a.ArticleOrder,
                IsPublished = a.IsPublished
            })
            .ToList()
    };
}
