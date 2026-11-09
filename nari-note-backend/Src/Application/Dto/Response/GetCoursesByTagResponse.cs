namespace NariNoteBackend.Application.Dto.Response;

public class GetCoursesByTagResponse
{
    public List<CourseDto> Courses { get; set; } = new();
    public int TotalCount { get; set; }
}
