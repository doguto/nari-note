using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetLikedArticlesResponse
{
    public UserId UserId { get; set; }
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
