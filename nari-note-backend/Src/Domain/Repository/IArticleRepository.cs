using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IArticleRepository : IRepository<Article, ArticleId>
{
    Task<List<Article>> FindByAuthorAsync(UserId authorId);
    Task<(List<ArticleSummary> Articles, int TotalCount)> FindSummariesByTagAsync(string tagName, int limit, int offset);
    Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null);
    Task<(List<Article> Articles, int TotalCount)> FindLatestSingleArticlesAsync(int limit, int offset);
    Task<List<Article>> FindDraftsByAuthorAsync(UserId authorId);
    Task<List<ArticleSummary>> SearchSummariesAsync(string keyword, int limit, int offset);
    Task<int> CountByAuthorAsync(UserId authorId);
}
