using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IArticleRepository : IRepository<Article, ArticleId>
{
    Task<List<Article>> FindByAuthorAsync(UserId authorId);
    Task<List<Article>> FindByTagAsync(string tagName);
    Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null);
    Task<(List<Article> Articles, int TotalCount)> FindLatestAsync(int limit, int offset);
    Task<List<Article>> FindDraftsByAuthorAsync(UserId authorId);
    Task<(List<Article> Articles, int TotalCount)> SearchAsync(string keyword, int limit, int offset);
}
