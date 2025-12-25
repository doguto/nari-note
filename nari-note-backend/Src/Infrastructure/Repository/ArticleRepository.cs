using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    private readonly NariNoteDbContext _context;
    
    public ArticleRepository(NariNoteDbContext context)
    {
        _context = context;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }
}
