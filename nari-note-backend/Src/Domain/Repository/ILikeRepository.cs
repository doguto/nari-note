using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ILikeRepository : IRepository<Like, LikeId>
{
    Task<Like?> FindByUserAndArticleAsync(UserId userId, ArticleId articleId);
    Task<int> CountByArticleAsync(ArticleId articleId);
    Task<List<Article>> FindLikedArticlesByUserAsync(UserId userId);
    Task<int> CountLikedArticlesByUserAsync(UserId userId);
}
