using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetMyArticlesService
{
    readonly IArticleRepository articleRepository;

    public GetMyArticlesService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<GetMyArticlesResponse> ExecuteAsync(UserId authorId)
    {
        var articles = await articleRepository.FindByAuthorAsync(authorId);

        var articleDtos = articles.Select(a => new ArticleThumbnailDto
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

        return new GetMyArticlesResponse
        {
            Articles = articleDtos
        };
    }
}
