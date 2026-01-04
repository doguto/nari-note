using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> FindByArticleAsync(int articleId);
    Task<List<Comment>> FindByUserAsync(int userId);
    Task<int> CountByArticleAsync(int articleId);
}
