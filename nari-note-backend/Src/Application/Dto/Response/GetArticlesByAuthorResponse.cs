using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByAuthorResponse
{
    public UserId AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
