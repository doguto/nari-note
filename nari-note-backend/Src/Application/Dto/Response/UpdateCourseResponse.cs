using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class UpdateCourseResponse
{
    public CourseId Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
