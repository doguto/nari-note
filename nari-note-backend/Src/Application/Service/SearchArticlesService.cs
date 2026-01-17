using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class SearchArticlesService
{
    readonly IArticleRepository articleRepository;
    
    public SearchArticlesService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<SearchArticlesResponse> ExecuteAsync(SearchArticlesRequest request)
    {
        var (articles, totalCount) = await articleRepository.SearchAsync(request.Keyword, request.Limit, request.Offset);

        var articleDtos = articles.Select(a => new ArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
            LikeCount = a.Likes.Count,
            IsPublished = a.IsPublished,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new SearchArticlesResponse
        {
            Articles = articleDtos,
            TotalCount = totalCount
        };
    }
}
