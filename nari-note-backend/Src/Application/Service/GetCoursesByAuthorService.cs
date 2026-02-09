using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetCoursesByAuthorService
{
    readonly ICourseRepository courseRepository;

    public GetCoursesByAuthorService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetCoursesByAuthorResponse> ExecuteAsync(GetCoursesByAuthorRequest request)
    {
        var courses = await courseRepository.FindByAuthorAsync(request.AuthorId);

        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.User.Name,
            Name = c.Name,
            ArticleIds = c.Articles.Select(a => a.Id).ToList(),
            ArticleNames = c.Articles.Select(a => a.Title).ToList(),
            LikeCount = c.LikeCount
        }).ToList();

        return new GetCoursesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = courses.FirstOrDefault()?.User.Name ?? "",
            Courses = courseDtos,
            TotalCount = courses.Count
        };
    }
}
