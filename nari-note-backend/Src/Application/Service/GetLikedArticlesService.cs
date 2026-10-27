using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetLikedArticlesService
{
    readonly ILikeRepository likeRepository;

    public GetLikedArticlesService(ILikeRepository likeRepository)
    {
        this.likeRepository = likeRepository;
    }

    public async Task<GetLikedArticlesResponse> ExecuteAsync(GetLikedArticlesRequest request)
    {
        PagingValidator.Validate(request.Limit, request.Offset);

        var (articles, totalCount) = await likeRepository.FindLikedArticleSummariesByUserAsync(request.UserId, request.Limit, request.Offset);

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

        return new GetLikedArticlesResponse
        {
            UserId = request.UserId,
            Articles = articleDtos,
            TotalCount = totalCount
        };
    }
}
