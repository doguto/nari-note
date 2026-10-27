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
        var (courses, totalCount) = await courseRepository.FindPublishedSummariesByAuthorAsync(request.AuthorId, request.Limit, request.Offset);

        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.UserName,
            UserIconImageUrl = c.UserProfileImage,
            Name = c.Name,
            ArticleIds = c.Articles.Select(a => a.Id).ToList(),
            ArticleNames = c.Articles.Select(a => a.Title).ToList(),
            LikeCount = c.LikeCount,
            IsPublished = c.IsPublished,
            PublishedAt = c.PublishedAt
        }).ToList();

        return new GetCoursesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = courses.FirstOrDefault()?.UserName ?? "",
            Courses = courseDtos,
            TotalCount = totalCount
        };
    }
}
