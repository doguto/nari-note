using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetLikedArticlesResponse
{
    public UserId UserId { get; set; }
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
