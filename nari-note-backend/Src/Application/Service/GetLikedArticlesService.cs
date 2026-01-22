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
        var articles = await likeRepository.FindLikedArticlesByUserAsync(request.UserId);

        var articleDtos = articles.Select(a => new ArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            AuthorId = a.AuthorId,
            AuthorName = a.Author.Name,
            Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
            LikeCount = a.Likes.Count,
            IsPublished = a.IsPublished,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new GetLikedArticlesResponse
        {
            UserId = request.UserId,
            Articles = articleDtos,
            TotalCount = articles.Count
        };
    }
}
