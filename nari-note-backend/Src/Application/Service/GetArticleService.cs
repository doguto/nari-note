using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetArticleService
{
    readonly IArticleRepository articleRepository;
    readonly ICommentRepository commentRepository;
    readonly ILikeRepository likeRepository;

    public GetArticleService(
        IArticleRepository articleRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository
    )
    {
        this.articleRepository = articleRepository;
        this.commentRepository = commentRepository;
        this.likeRepository = likeRepository;
    }

    public async Task<GetArticleResponse> ExecuteAsync(GetArticleRequest request, UserId? userId = null)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        var comments = await commentRepository.FindByArticleAsync(request.Id);

        var isLiked = false;
        if (userId.HasValue)
        {
            var like = await likeRepository.FindByUserAndArticleAsync(userId.Value, request.Id);
            isLiked = like != null;
        }

        return new GetArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Body = article.Body,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.Name ?? "",
            Tags = article.ArticleTags.Select(at => at.Tag?.Name ?? string.Empty)
                          .Where(name => !name.IsNullOrEmpty()).ToList(),
            LikeCount = article.LikeCount,
            IsLiked = isLiked,
            IsPublished = article.IsPublished,
            PublishedAt = article.PublishedAt,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Comments = comments
                       .Where(c => !string.IsNullOrEmpty(c.User?.Name))
                       .Select(c => new CommentDto
                       {
                           Id = c.Id,
                           UserId = c.UserId,
                           UserName = c.User!.Name,
                           Message = c.Message,
                           CreatedAt = c.CreatedAt
                       }).OrderBy(c => c.CreatedAt).ToList()
        };
    }
}
