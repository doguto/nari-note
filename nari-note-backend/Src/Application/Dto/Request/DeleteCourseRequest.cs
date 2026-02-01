using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeleteCourseRequest
{
    public CourseId Id { get; set; }
}
