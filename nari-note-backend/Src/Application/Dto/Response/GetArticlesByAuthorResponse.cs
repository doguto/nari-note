namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByAuthorResponse
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
