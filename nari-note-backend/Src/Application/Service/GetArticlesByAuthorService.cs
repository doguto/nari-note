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
        var (articles, totalCount) = await articleRepository.FindPublishedSummariesByAuthorAsync(request.AuthorId, request.Limit, request.Offset);

        var articleDtos = articles.Select(a => new ArticleThumbnailDto
        {
            Id = a.Id,
            Title = a.Title,
            AuthorId = a.AuthorId,
            AuthorName = a.AuthorName,
            UserIconImageUrl = a.AuthorProfileImage,
            Tags = a.TagNames,
            LikeCount = a.LikeCount,
            IsPublished = a.IsPublished,
            PublishedAt = a.PublishedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new GetArticlesByAuthorResponse
        {
            AuthorId = request.AuthorId,
            AuthorName = articles.FirstOrDefault()?.AuthorName ?? "",
            Articles = articleDtos,
            TotalCount = totalCount
        };
    }
}
