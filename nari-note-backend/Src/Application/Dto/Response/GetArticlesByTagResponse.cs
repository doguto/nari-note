using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class GetArticlesByTagResponse
{
    public List<ArticleDto> Articles { get; set; } = new();
}
