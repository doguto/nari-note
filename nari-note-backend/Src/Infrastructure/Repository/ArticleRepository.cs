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
            this.context.Articles.Add(article);
            await this.context.SaveChangesAsync();
            return article;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            // DB制約違反を適切な例外に変換
            if (pgEx.SqlState == "23505") // Unique constraint violation
            {
                throw new ConflictException(
                    "Article with this title already exists",
                    ex);
            }
            if (pgEx.SqlState == "23503") // Foreign key violation
            {
                throw new ValidationException(
                    "Invalid reference to related entity",
                    null,
                    ex);
            }
            // その他のDB例外
            throw new InfrastructureException(
                "Database error occurred while creating article",
                ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException(
                "The article was modified by another user",
                ex);
        }
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await this.context.Articles
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var article = await this.context.Articles.FindAsync(id);
            if (article != null)
            {
                this.context.Articles.Remove(article);
                await this.context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException(
                "The article was modified or deleted by another user",
                ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                $"Database error occurred while deleting article with ID {id}",
                ex);
        }
    }
}
