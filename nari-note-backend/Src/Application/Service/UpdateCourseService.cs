using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateCourseService
{
    readonly ICourseRepository courseRepository;
    readonly TimeProvider timeProvider;

    public UpdateCourseService(ICourseRepository courseRepository, TimeProvider timeProvider)
    {
        this.courseRepository = courseRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UpdateCourseResponse> ExecuteAsync(UserId userId, UpdateCourseRequest request)
    {
        var course = await courseRepository.FindForceByIdAsync(request.Id);
        if (course.UserId != userId) throw new ForbiddenException("この講座を更新する権限がありません");

        if (!request.Name.IsNullOrEmpty()) course.Name = request.Name!;

        var previousPublishedAt = course.PublishedAt;
        var wasPubliclyVisible = course.IsPubliclyVisibleAt(timeProvider.UtcNow());

        // 予約公開中の講座を即時公開した場合も現在時刻を公開日時にする
        if (request.PublishedAt.HasValue)
        {
            course.PublishedAt = request.PublishedAt.Value;
        }
        else if (!wasPubliclyVisible && request.IsPublished.HasValue && request.IsPublished.Value)
        {
            course.PublishedAt = timeProvider.UtcNow();
        }

        course.UpdatedAt = timeProvider.UtcNow();

        await courseRepository.UpdateWithArticlesAsync(course, previousPublishedAt);

        return new UpdateCourseResponse
        {
            Id = course.Id,
            UpdatedAt = course.UpdatedAt
        };
    }
}
