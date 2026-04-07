using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByAuthorResponse
{
    public UserId AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
