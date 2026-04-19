using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetMyCoursesService
{
    readonly ICourseRepository courseRepository;

    public GetMyCoursesService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetMyCoursesResponse> ExecuteAsync(UserId authorId)
    {
        var courses = await courseRepository.FindAllByAuthorAsync(authorId);

        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.User.Name,
            UserProfileImage = c.User.ProfileImage,
            Name = c.Name,
            ArticleIds = c.Articles.Select(a => a.Id).ToList(),
            ArticleNames = c.Articles.Select(a => a.Title).ToList(),
            LikeCount = c.LikeCount,
            IsPublished = c.IsPublished,
            PublishedAt = c.PublishedAt
        }).ToList();

        return new GetMyCoursesResponse
        {
            Courses = courseDtos,
            TotalCount = courses.Count
        };
    }
}
