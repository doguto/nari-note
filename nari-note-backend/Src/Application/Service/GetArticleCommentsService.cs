using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetArticleCommentsService
{
    readonly IArticleRepository articleRepository;
    readonly ICommentRepository commentRepository;
    readonly TimeProvider timeProvider;

    public GetArticleCommentsService(
        IArticleRepository articleRepository,
        ICommentRepository commentRepository,
        TimeProvider timeProvider
    )
    {
        this.articleRepository = articleRepository;
        this.commentRepository = commentRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<GetArticleCommentsResponse> ExecuteAsync(GetArticleCommentsRequest request, UserId? userId = null)
    {
        var article = await this.articleRepository.FindForceByIdAsync(request.Id);

        // 記事本体と同じく、未公開の記事のコメントは作者本人以外には存在自体を秘匿する
        var isAuthor = userId.HasValue && article.AuthorId == userId.Value;
        if (!article.IsPubliclyVisibleAt(this.timeProvider.UtcNow()) && !isAuthor) throw new KeyNotFoundException($"記事{request.Id}が存在しません");

        var (comments, totalCount) = await this.commentRepository.FindPageByArticleAsync(request.Id, request.Limit, request.Offset);

        return new GetArticleCommentsResponse
        {
            Comments = comments.Select(c => new CommentDto
                               {
                                   Id = c.Id,
                                   UserId = c.UserId,
                                   UserName = c.User.Name,
                                   UserIconImageUrl = c.User.ProfileImage,
                                   Message = c.Message,
                                   CreatedAt = c.CreatedAt
                               })
                               .ToList(),
            TotalCount = totalCount
        };
    }
}
