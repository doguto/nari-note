using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository : IRepository<Article>
{
    Task<List<Article>> FindByAuthorAsync(int authorId);
    Task<List<Article>> FindByTagAsync(string tagName);
    Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null);
}
