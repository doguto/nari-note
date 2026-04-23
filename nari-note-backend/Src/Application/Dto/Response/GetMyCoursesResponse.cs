namespace NariNoteBackend.Application.Dto.Response;

public class GetMyCoursesResponse
{
    public List<CourseDto> Courses { get; set; } = new();
}
