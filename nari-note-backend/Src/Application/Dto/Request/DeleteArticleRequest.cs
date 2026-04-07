using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class DeleteArticleRequest
{
    public ArticleId Id { get; set; }
}
