using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetCoursesByTagService
{
    readonly ICourseRepository courseRepository;

    public GetCoursesByTagService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetCoursesByTagResponse> ExecuteAsync(GetCoursesByTagRequest request)
    {
        var (courses, totalCount) = await this.courseRepository.FindPublishedSummariesByTagAsync(request.TagName, request.Limit, request.Offset);

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

        return new GetCoursesByTagResponse
        {
            Courses = courseDtos,
            TotalCount = totalCount
        };
    }
}
