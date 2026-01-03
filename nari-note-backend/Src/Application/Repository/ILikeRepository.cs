using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface ILikeRepository : IRepository<Like>
{
    Task<Like?> FindByUserAndArticleAsync(int userId, int articleId);
    Task<int> CountByArticleAsync(int articleId);
}
