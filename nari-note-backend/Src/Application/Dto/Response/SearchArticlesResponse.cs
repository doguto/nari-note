namespace NariNoteBackend.Application.Dto.Response;

public class SearchArticlesResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
