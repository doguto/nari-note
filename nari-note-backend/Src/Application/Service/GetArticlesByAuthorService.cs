using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByAuthorService
{
    readonly IArticleRepository articleRepository;

    public GetArticlesByAuthorService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<GetArticlesByAuthorResponse> ExecuteAsync(GetArticlesByAuthorRequest request)
    {
        var articles = await articleRepository.FindByAuthorAsync(request.AuthorId);
        var publishedArticles = articles.Where(a => a.IsPublished).ToList();

        var articleDtos = publishedArticles.Select(a => new ArticleThumbnailDto
        {
            Id = a.Id,
            Title = a.Title,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            UserIconImageUrl = a.Author.ProfileImage,
            Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
            LikeCount = a.Likes.Count,
            IsPublished = a.IsPublished,
            PublishedAt = a.PublishedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new GetArticlesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = publishedArticles.FirstOrDefault()?.Author.Name ?? "",
            Articles = articleDtos
        };
    }
}
