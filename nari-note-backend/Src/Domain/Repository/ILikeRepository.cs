using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface ILikeRepository : IRepository<Like>
{
    Task<Like?> FindByUserAndArticleAsync(int userId, int articleId);
    Task<int> CountByArticleAsync(int articleId);
}
