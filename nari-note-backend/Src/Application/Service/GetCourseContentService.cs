using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetCourseContentService
{
    readonly ICourseRepository courseRepository;
    readonly TimeProvider timeProvider;

    public GetCourseContentService(ICourseRepository courseRepository, TimeProvider timeProvider)
    {
        this.courseRepository = courseRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<GetCourseContentResponse> ExecuteAsync(GetCourseContentRequest request, UserId? userId = null)
    {
        var course = await courseRepository.FindByIdWithArticlesAsync(request.Id);

        // 未公開（下書き・予約公開）の講座は作成者本人以外には存在自体を秘匿する
        var isOwner = userId.HasValue && course.UserId == userId.Value;
        if (!course.IsPubliclyVisibleAt(timeProvider.UtcNow()) && !isOwner) throw new KeyNotFoundException($"ID: {request.Id} の講座が見つかりません");
        return GetCourseContentResponse.From(course);
    }
}
