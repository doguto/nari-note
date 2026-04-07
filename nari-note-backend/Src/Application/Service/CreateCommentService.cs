using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class CreateCommentService
{
    readonly ICommentRepository commentRepository;
    readonly IArticleRepository articleRepository;
    
    public CreateCommentService(
        ICommentRepository commentRepository,
        IArticleRepository articleRepository)
    {
        this.commentRepository = commentRepository;
        this.articleRepository = articleRepository;
    }
    
    public async Task<CreateCommentResponse> ExecuteAsync(UserId userId, CreateCommentRequest request)
    {
        // 記事の存在確認
        await articleRepository.FindForceByIdAsync(request.ArticleId);
        
        // コメントの作成
        var comment = new Comment
        {
            UserId = userId,
            ArticleId = request.ArticleId,
            Message = request.Message
        };
        
        var createdComment = await commentRepository.CreateAsync(comment);
        
        return new CreateCommentResponse
        {
            Id = createdComment.Id,
            CreatedAt = createdComment.CreatedAt
        };
    }
}
