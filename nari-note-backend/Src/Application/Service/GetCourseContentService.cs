using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

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

        return new GetCourseContentResponse
        {
            Id = course.Id,
            Name = course.Name,
            UserId = course.UserId,
            UserName = course.User?.Name ?? "",
            LikeCount = course.LikeCount,
            IsPublished = course.IsPublished,
            PublishedAt = course.PublishedAt,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            Articles = course.Articles
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
}
