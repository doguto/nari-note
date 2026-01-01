using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository
{
    Task<Article> CreateAsync(Article article);
    Task<List<Article>> FindByAuthorAsync(int authorId);
    Task<List<Article>> FindByTagAsync(string tagName);
    Task<Article?> FindByIdAsync(int id);
    Task<Article> GetByIdAsync(int id);
    Task<Article> UpdateAsync(Article article);
    Task UpdateArticleTagsAsync(int articleId, List<string> tagNames);
    Task DeleteAsync(int id);
}
