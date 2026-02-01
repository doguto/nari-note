using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateCourseService
{
    readonly ICourseRepository courseRepository;

    public UpdateCourseService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<UpdateCourseResponse> ExecuteAsync(UserId userId, UpdateCourseRequest request)
    {
        var course = await courseRepository.FindForceByIdAsync(request.Id);
        if (course.UserId != userId) throw new UnauthorizedAccessException("この講座を更新する権限がありません");

        if (!request.Name.IsNullOrEmpty()) course.Name = request.Name!;

        var wasPublished = course.IsPublished;

        if (request.PublishedAt.HasValue)
        {
            course.PublishedAt = request.PublishedAt.Value;
        }
        else if (!wasPublished && request.IsPublished.HasValue && request.IsPublished.Value)
        {
            course.PublishedAt = DateTime.UtcNow;
        }

        course.UpdatedAt = DateTime.UtcNow;

        await courseRepository.UpdateWithArticlesAsync(course);

        return new UpdateCourseResponse
        {
            Id = course.Id,
            UpdatedAt = course.UpdatedAt
        };
    }
}
