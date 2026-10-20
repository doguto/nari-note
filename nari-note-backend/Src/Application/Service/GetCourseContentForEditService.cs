using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetCourseContentForEditService
{
    readonly ICourseRepository courseRepository;

    public GetCourseContentForEditService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task<GetCourseContentResponse> ExecuteAsync(UserId requesterId, GetCourseContentRequest request)
    {
        var course = await courseRepository.FindByIdWithAllArticlesAsync(request.Id);

        if (course.UserId != requesterId)
            throw new ForbiddenException("この講座を編集する権限がありません");

        return GetCourseContentResponse.From(course);
    }
}
