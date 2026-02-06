using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class SearchCoursesService
{
    readonly ICourseRepository courseRepository;

    public SearchCoursesService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<SearchCoursesResponse> ExecuteAsync(SearchCoursesRequest request)
    {
        var courses = await courseRepository.SearchAsync(request.Keyword, request.Limit, request.Offset);

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

        return new SearchCoursesResponse
        {
            Courses = courseDtos
        };
    }
}
