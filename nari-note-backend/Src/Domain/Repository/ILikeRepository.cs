using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ILikeRepository : IRepository<Like, LikeId>
{
    Task<Like?> FindByUserAndArticleAsync(UserId userId, ArticleId articleId);
    Task<int> CountByArticleAsync(ArticleId articleId);
    Task<(List<ArticleSummary> Articles, int TotalCount)> FindLikedArticleSummariesByUserAsync(UserId userId, int limit, int offset);
    Task<int> CountLikedArticlesByUserAsync(UserId userId);
}
