using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetCourseContentService
{
    readonly ICourseRepository courseRepository;
    readonly TimeProvider timeProvider;

    public GetCourseContentService(ICourseRepository courseRepository, TimeProvider timeProvider)
    {
        this.courseRepository = courseRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<GetCourseContentResponse> ExecuteAsync(GetCourseContentRequest request, UserId? userId = null)
    {
        var course = await courseRepository.FindByIdWithArticlesAsync(request.Id);

        // 未公開（下書き・予約公開）の講座は作成者本人以外には存在自体を秘匿する
        var isOwner = userId.HasValue && course.UserId == userId.Value;
        if (!course.IsPubliclyVisibleAt(timeProvider.UtcNow()) && !isOwner) throw new KeyNotFoundException($"ID: {request.Id} の講座が見つかりません");
        return MapToResponse(course);
    }

    public async Task<GetCourseContentResponse> ExecuteForEditAsync(UserId requesterId, GetCourseContentRequest request)
    {
        var course = await courseRepository.FindByIdWithAllArticlesAsync(request.Id);

        if (course.UserId != requesterId)
            throw new ForbiddenException("この講座を編集する権限がありません");

        return MapToResponse(course);
    }

    GetCourseContentResponse MapToResponse(Domain.Entity.Course course) => new()
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
