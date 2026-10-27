namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
