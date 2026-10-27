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
        // 1 件多く取得し、はみ出した分があれば次のページがあると判定する
        var articles = await articleRepository.SearchSummariesAsync(request.Keyword, request.Limit + 1, request.Offset);
        var hasNext = articles.Count > request.Limit;

        var articleDtos = articles.Take(request.Limit).Select(a => new ArticleThumbnailDto
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

        return new SearchArticlesResponse
        {
            Articles = articleDtos,
            HasNext = hasNext
        };
    }
}
