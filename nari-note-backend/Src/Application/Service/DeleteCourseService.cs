using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class DeleteCourseService
{
    readonly ICourseRepository courseRepository;
    
    public DeleteCourseService(ICourseRepository courseRepository)
    {
        this.courseRepository = courseRepository;
    }

    public async Task ExecuteAsync(UserId userId, DeleteCourseRequest request)
    {
        var course = await courseRepository.FindForceByIdAsync(request.Id);
        if (course.UserId != userId) throw new UnauthorizedAccessException("この講座を削除する権限がありません");
            
        await courseRepository.DeleteAsync(request.Id);
    }
}
