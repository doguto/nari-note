using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

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

    public async Task<Comment?> FindByIdAsync(CommentId id)
    {
        return await context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Comment> FindForceByIdAsync(CommentId id)
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

    public async Task DeleteAsync(CommentId id)
    {
        var comment = await context.Comments.FindAsync(id);
        if (comment == null) return;

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }

    public async Task<List<Comment>> FindByArticleAsync(ArticleId articleId)
    {
        return await context.Comments
            .Include(c => c.User)
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Comment>> FindByUserAsync(UserId userId)
    {
        return await context.Comments
            .Include(c => c.Article)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByArticleAsync(ArticleId articleId)
    {
        return await context.Comments.CountAsync(c => c.ArticleId == articleId);
    }
}
