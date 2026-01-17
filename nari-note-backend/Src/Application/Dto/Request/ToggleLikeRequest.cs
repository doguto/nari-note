using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class ToggleLikeRequest
{
    public ArticleId ArticleId { get; set; }
}
