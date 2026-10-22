using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByAuthorService
{
    readonly IArticleRepository articleRepository;
    readonly TimeProvider timeProvider;

    public GetArticlesByAuthorService(IArticleRepository articleRepository, TimeProvider timeProvider)
    {
        this.articleRepository = articleRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<GetArticlesByAuthorResponse> ExecuteAsync(GetArticlesByAuthorRequest request)
    {
        var articles = await articleRepository.FindByAuthorAsync(request.AuthorId);
        // 下書きに加え、公開日時を過ぎていない予約投稿も第三者には見せない
        var now = timeProvider.UtcNow();
        var publishedArticles = articles.Where(a => a.IsPubliclyVisibleAt(now)).ToList();

        var articleDtos = publishedArticles.Select(a => new ArticleThumbnailDto
        {
            Id = a.Id,
            Title = a.Title,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            UserIconImageUrl = a.Author.ProfileImage,
            Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
            LikeCount = a.LikeCount,
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
