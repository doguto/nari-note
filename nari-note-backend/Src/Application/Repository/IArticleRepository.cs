using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository
{
    Task<Article> CreateAsync(Article article);
    Task<List<Article>> FindByAuthorAsync(int authorId);
    Task<Article?> FindByIdAsync(int id);
    Task<Article> GetByIdAsync(int id);
    Task<Article> UpdateAsync(Article article);
    Task DeleteAsync(int id);
}
