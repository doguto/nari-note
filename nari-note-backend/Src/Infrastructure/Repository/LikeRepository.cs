using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class LikeRepository : ILikeRepository
{
    readonly NariNoteDbContext context;
    
    public LikeRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Like?> FindByUserAndArticleAsync(int userId, int articleId)
    {
        return await this.context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ArticleId == articleId);
    }
    
    public async Task<Like> CreateAsync(Like like)
    {
        try
        {
            this.context.Likes.Add(like);
            await this.context.SaveChangesAsync();
            return like;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("Like already exists", ex);
            }
            if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new ValidationException("Invalid reference to related entity", null, ex);
            }
            throw new InfrastructureException("Database error occurred while creating like", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The like was modified by another user", ex);
        }
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var like = await this.context.Likes.FindAsync(id);
            if (like != null)
            {
                this.context.Likes.Remove(like);
                await this.context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The like was modified or deleted by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException($"Database error occurred while deleting like with ID {id}", ex);
        }
    }
    
    public async Task<int> CountByArticleAsync(int articleId)
    {
        return await this.context.Likes
            .CountAsync(l => l.ArticleId == articleId);
    }
}
