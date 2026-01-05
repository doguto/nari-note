namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesResponse
{
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
