namespace NariNoteBackend.Application.Dto.Response;

public class SearchCoursesResponse
{
    public List<CourseDto> Courses { get; set; } = new();

    // 総件数は数えず、次のページがあるかどうかだけを返す
    public bool HasNext { get; set; }
}
