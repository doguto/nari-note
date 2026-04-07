namespace NariNoteBackend.Application.Dto.Request;

public class GetArticlesRequest
{
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
