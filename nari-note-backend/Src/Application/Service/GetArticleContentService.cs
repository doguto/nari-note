using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class GetArticleContentService
{
    readonly IArticleRepository articleRepository;
    readonly ICommentRepository commentRepository;
    readonly ILikeRepository likeRepository;

    public GetArticleContentService(
        IArticleRepository articleRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository
    )
    {
        this.articleRepository = articleRepository;
        this.commentRepository = commentRepository;
        this.likeRepository = likeRepository;
    }

    public async Task<GetArticleContentResponse> ExecuteAsync(GetArticleContentRequest request, UserId? userId = null)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        var comments = await commentRepository.FindByArticleAsync(request.Id);

        var isLiked = false;
        if (userId.HasValue)
        {
            var like = await likeRepository.FindByUserAndArticleAsync(userId.Value, request.Id);
            isLiked = like != null;
        }

        return new GetArticleContentResponse
        {
            Article = new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Body = article.Body,
                AuthorId = article.AuthorId,
                AuthorName = article.Author.Name,
                Tags = article.ArticleTags.Select(x => x.Tag.Name).ToList(),
                Kifus = article.Kifus.Select(x => new KifuDto { KifuText = x.KifuText, SortOrder = x.SortOrder })
                               .ToList(),
                LikeCount = article.LikeCount,
                IsPublished = article.IsPublished,
                PublishedAt = article.PublishedAt,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            },
            IsLiked = isLiked,
            CourseId = article.CourseId,
            CourseName = article.Course?.Name,
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
