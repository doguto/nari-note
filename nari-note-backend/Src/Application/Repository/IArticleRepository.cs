using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository
{
    Task<Article> CreateAsync(Article article);
}
