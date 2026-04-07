using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetCoursesService
{
    readonly ICourseRepository courseRepository;

    public GetCoursesService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetCoursesResponse> ExecuteAsync(GetCoursesRequest request)
    {
        var (courses, totalCount) = await courseRepository.FindLatestAsync(request.Limit, request.Offset);

        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.User.Name,
            Name = c.Name,
            ArticleIds = c.Articles.Select(a => a.Id).ToList(),
            ArticleNames = c.Articles.Select(a => a.Title).ToList(),
            LikeCount = c.LikeCount,
            IsPublished = c.IsPublished,
            PublishedAt = c.PublishedAt
        }).ToList();

        return new GetCoursesResponse
        {
            Courses = courseDtos,
            TotalCount = totalCount
        };
    }
}
