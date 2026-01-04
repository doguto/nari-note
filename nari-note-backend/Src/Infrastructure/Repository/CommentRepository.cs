using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Infrastructure.Repository;

public class CommentRepository : ICommentRepository
{
    readonly NariNoteDbContext context;
    
    public CommentRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Comment> CreateAsync(Comment comment)
    {
        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> FindByIdAsync(int id)
    {
        return await context.Comments
            .Include(c => c.User)
            .Include(c => c.Article)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Comment> FindForceByIdAsync(int id)
    {
        var comment = await FindByIdAsync(id);
        if (comment == null)
        {
            throw new KeyNotFoundException($"ID: {id} のコメントが見つかりません");
        }
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment entity)
    {
        context.Comments.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var comment = await context.Comments.FindAsync(id);
        if (comment == null) return;

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }

    public async Task<List<Comment>> FindByArticleAsync(int articleId)
    {
        return await context.Comments
            .Include(c => c.User)
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Comment>> FindByUserAsync(int userId)
    {
        return await context.Comments
            .Include(c => c.Article)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByArticleAsync(int articleId)
    {
        return await context.Comments.CountAsync(c => c.ArticleId == articleId);
    }
}
