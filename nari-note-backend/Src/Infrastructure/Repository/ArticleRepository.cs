using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    readonly NariNoteDbContext context;
    
    public ArticleRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        try
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
            return article;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("Article with this title already exists", ex);
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new ValidationException("Invalid reference to related entity", null, ex);
            }
            throw new InfrastructureException("Database error occurred while creating article", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified by another user", ex);
        }
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await context.Articles
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Article> GetByIdAsync(int id)
    {
        var article = await FindByIdAsync(id);
        
        if (article == null)
        {
            throw new NotFoundException($"Article with ID {id} not found");
        }
        
        return article;
    }
    
    public async Task<List<Article>> FindByAuthorAsync(int authorId)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Where(a => a.AuthorId == authorId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var article = await context.Articles.FindAsync(id);
            if (article != null)
            {
                context.Articles.Remove(article);
                await context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified or deleted by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException($"Database error occurred while deleting article with ID {id}", ex);
        }
    }
}
