using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesService
{
    readonly IArticleRepository articleRepository;
    
    public GetArticlesService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<GetArticlesResponse> ExecuteAsync(GetArticlesRequest request)
    {
        var (articles, totalCount) = await articleRepository.FindLatestAsync(request.Limit, request.Offset);

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

        return new GetArticlesResponse
        {
            Articles = articleDtos,
            TotalCount = totalCount
        };
    }
}
