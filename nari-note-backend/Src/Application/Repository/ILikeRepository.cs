using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface ILikeRepository
{
    Task<Like?> FindByUserAndArticleAsync(int userId, int articleId);
    Task<Like> CreateAsync(Like like);
    Task DeleteAsync(int id);
    Task<int> CountByArticleAsync(int articleId);
}
