namespace NariNoteBackend.Application.Dto.Request;

public class GetCoursesRequest
{
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
