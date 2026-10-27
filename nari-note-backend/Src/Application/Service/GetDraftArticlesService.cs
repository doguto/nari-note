using NariNoteBackend.Application.Dto;
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
        var articles = await articleRepository.FindDraftSummariesByAuthorAsync(authorId);

        var articleDtos = articles.Select(a => new ArticleThumbnailDto
        {
            Id = a.Id,
            Title = a.Title,
            AuthorId = a.AuthorId,
            AuthorName = a.AuthorName,
            UserIconImageUrl = a.AuthorProfileImage,
            Tags = a.TagNames,
            LikeCount = 0, // 下書きにはLikeが存在しない
            IsPublished = a.IsPublished,
            PublishedAt = a.PublishedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new GetDraftArticlesResponse
        {
            Articles = articleDtos
        };
    }
}
