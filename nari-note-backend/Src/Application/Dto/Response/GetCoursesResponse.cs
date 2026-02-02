namespace NariNoteBackend.Application.Dto.Response;

public class GetCoursesResponse
{
    public List<CourseDto> Courses { get; set; } = new();
    public int TotalCount { get; set; }
}
