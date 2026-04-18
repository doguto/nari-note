namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByTagResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();
}
