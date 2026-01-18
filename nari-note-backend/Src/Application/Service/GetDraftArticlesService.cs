using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetDraftArticlesService
{
    readonly IArticleRepository articleRepository;
    
    public GetDraftArticlesService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<GetDraftArticlesResponse> ExecuteAsync(UserId authorId)
    {
        var articles = await articleRepository.FindDraftsByAuthorAsync(authorId);

        var articleDtos = articles.Select(a => new ArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
            LikeCount = 0, // 下書きにはLikeが存在しない
            IsPublished = a.IsPublished,
            PublishedAt = a.PublishedAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new GetDraftArticlesResponse
        {
            Articles = articleDtos
        };
    }
}
