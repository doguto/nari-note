using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ILikeRepository : IRepository<Like, LikeId>
{
    Task<Like?> FindByUserAndArticleAsync(UserId userId, ArticleId articleId);
    Task<int> CountByArticleAsync(ArticleId articleId);
}
