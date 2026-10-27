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
        var articles = await articleRepository.FindLatestSingleSummariesAsync(request.Limit, request.Offset);

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

        return new GetArticlesResponse
        {
            Articles = articleDtos
        };
    }
}
