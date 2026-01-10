using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface IArticleRepository : IRepository<Article>
{
    Task<List<Article>> FindByAuthorAsync(int authorId);
    Task<List<Article>> FindByTagAsync(string tagName);
    Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null);
    Task<(List<Article> Articles, int TotalCount)> FindLatestAsync(int limit, int offset);
    Task<List<Article>> FindDraftsByAuthorAsync(int authorId);
}
