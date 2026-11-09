namespace NariNoteBackend.Application.Dto.Request;

public class GetCoursesByTagRequest
{
    public required string TagName { get; set; }
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
