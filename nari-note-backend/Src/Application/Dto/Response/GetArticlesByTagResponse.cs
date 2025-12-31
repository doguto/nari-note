using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByTagResponse
{
    public string TagName { get; set; } = string.Empty;
    public List<ArticleDto> Articles { get; set; } = new();
    public int TotalCount { get; set; }
}
