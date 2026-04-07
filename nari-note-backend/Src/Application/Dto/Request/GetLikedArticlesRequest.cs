using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class GetLikedArticlesRequest
{
    public UserId UserId { get; set; }
}
