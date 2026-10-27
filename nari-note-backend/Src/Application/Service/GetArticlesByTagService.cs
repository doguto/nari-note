using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByTagService
{
    static readonly Regex ValidTagPattern = new(@"^[a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FFF_\-\.]+$", RegexOptions.Compiled);
    const int MaxLimit = 100;

    readonly IArticleRepository articleRepository;

    public GetArticlesByTagService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<GetArticlesByTagResponse> ExecuteAsync(GetArticlesByTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagName) || !ValidTagPattern.IsMatch(request.TagName))
        {
            throw new ValidationException("タグ名に使用できない文字が含まれています");
        }

        if (request.Limit is < 1 or > MaxLimit)
        {
            throw new ValidationException($"取得件数は1から{MaxLimit}の範囲で指定してください");
        }

        if (request.Offset < 0)
        {
            throw new ValidationException("オフセットは0以上の値を指定してください");
        }

        var (articles, totalCount) = await articleRepository.FindSummariesByTagAsync(request.TagName, request.Limit, request.Offset);

        return new GetArticlesByTagResponse
        {
            Articles = articles.Select(a => new ArticleThumbnailDto
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
            }).ToList(),
            TotalCount = totalCount
        };
    }
}
