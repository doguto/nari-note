using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetCourseContentService
{
    readonly ICourseRepository courseRepository;

    public GetCourseContentService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetCourseContentResponse> ExecuteAsync(GetCourseContentRequest request)
    {
        var course = await courseRepository.FindByIdWithArticlesAsync(request.Id);
        return MapToResponse(course);
    }

    public async Task<GetCourseContentResponse> ExecuteForEditAsync(UserId requesterId, GetCourseContentRequest request)
    {
        var course = await courseRepository.FindByIdWithAllArticlesAsync(request.Id);

        if (course.UserId != requesterId)
            throw new UnauthorizedAccessException("この講座を編集する権限がありません");

        return MapToResponse(course);
    }

    GetCourseContentResponse MapToResponse(Domain.Entity.Course course) => new()
    {
        Id = course.Id,
        Name = course.Name,
        UserId = course.UserId,
        UserName = course.User?.Name ?? "",
        UserProfileImage = course.User?.ProfileImage,
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
