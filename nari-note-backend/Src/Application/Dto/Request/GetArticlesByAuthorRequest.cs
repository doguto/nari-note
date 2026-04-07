using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetArticlesByAuthorRequest
{
    public UserId AuthorId { get; set; }
}
