using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class GetMyArticlesResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
