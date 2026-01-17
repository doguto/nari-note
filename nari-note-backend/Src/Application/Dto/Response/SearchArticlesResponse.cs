namespace NariNoteBackend.Application.Dto.Response;

public class SearchArticlesResponse
{
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
