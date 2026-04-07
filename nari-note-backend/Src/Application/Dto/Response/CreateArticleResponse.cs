using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class CreateArticleResponse
{
    public ArticleId Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
