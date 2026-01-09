namespace NariNoteBackend.Application.Dto.Response;

public class GetDraftArticlesResponse
{
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
