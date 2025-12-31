using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticleService
{
    readonly IArticleRepository articleRepository;

    public GetArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<GetArticleResponse?> ExecuteAsync(GetArticleRequest request)
    {
        var article = await this.articleRepository.FindByIdAsync(request.Id);
        if (article == null) return null;
        
        return new GetArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Body = article.Body,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.Name ?? "",
            Tags = article.ArticleTags.Select(at => at.Tag?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name)).ToList(),
            LikeCount = article.LikeCount,
            IsPublished = article.IsPublished,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
