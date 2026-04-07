using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ICommentRepository : IRepository<Comment, CommentId>
{
    Task<List<Comment>> FindByArticleAsync(ArticleId articleId);
    Task<List<Comment>> FindByUserAsync(UserId userId);
    Task<int> CountByArticleAsync(ArticleId articleId);
}
