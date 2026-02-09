using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetCoursesByAuthorResponse
{
    public UserId AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public List<CourseDto> Courses { get; set; } = new();
    public int TotalCount { get; set; }
}
