namespace NariNoteBackend.Application.Dto.Response;

public class GetDraftArticlesResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
