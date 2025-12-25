using Microsoft.EntityFrameworkCore;
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
        this.context.Articles.Add(article);
        await this.context.SaveChangesAsync();
        return article;
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await this.context.Articles.FindAsync(id);
    }
    
    public async Task DeleteAsync(int id)
    {
        var article = await this.context.Articles.FindAsync(id);
        if (article != null)
        {
            this.context.Articles.Remove(article);
            await this.context.SaveChangesAsync();
        }
    }
}
