using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetCourseContentRequest
{
    public CourseId Id { get; set; }
}
