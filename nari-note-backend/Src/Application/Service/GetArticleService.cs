using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class GetArticleService
{
    readonly IArticleRepository articleRepository;

    public GetArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<Article> ExecuteAsync(int id)
    {
        var article = await articleRepository.GetByIdAsync(id);
        return article;
    }
}
