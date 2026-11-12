using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ICommentRepository : IRepository<Comment, CommentId>
{
    Task<(List<Comment> Comments, int TotalCount)> FindPageByArticleAsync(ArticleId articleId, int limit, int offset);
    Task<List<Comment>> FindByUserAsync(UserId userId);
    Task<int> CountByArticleAsync(ArticleId articleId);
}
