using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    private readonly NariNoteDbContext context;
    
    public ArticleRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }
}
