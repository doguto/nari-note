namespace NariNoteBackend.Application.Dto.Request;

public class GetArticlesByTagRequest
{
    public string TagName { get; set; } = string.Empty;
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
