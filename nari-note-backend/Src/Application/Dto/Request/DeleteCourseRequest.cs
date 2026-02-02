using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeleteCourseRequest
{
    public required CourseId Id { get; set; }
}
