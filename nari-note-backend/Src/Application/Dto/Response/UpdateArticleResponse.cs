using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class UpdateArticleResponse
{
    public ArticleId Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
