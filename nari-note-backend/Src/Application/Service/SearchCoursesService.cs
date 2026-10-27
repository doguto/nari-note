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
        // 1 件多く取得し、はみ出した分があれば次のページがあると判定する
        var courses = await courseRepository.SearchAsync(request.Keyword, request.Limit + 1, request.Offset);
        var hasNext = courses.Count > request.Limit;

        var courseDtos = courses.Take(request.Limit).Select(c => new CourseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.User.Name,
            UserIconImageUrl = c.User.ProfileImage,
            Name = c.Name,
            ArticleIds = c.Articles.Select(a => a.Id).ToList(),
            ArticleNames = c.Articles.Select(a => a.Title).ToList(),
            LikeCount = c.LikeCount,
            IsPublished = c.IsPublished,
            PublishedAt = c.PublishedAt
        }).ToList();

        return new SearchCoursesResponse
        {
            Courses = courseDtos,
            HasNext = hasNext
        };
    }
}
