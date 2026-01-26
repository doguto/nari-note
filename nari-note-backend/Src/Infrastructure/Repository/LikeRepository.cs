using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class LikeRepository : ILikeRepository
{
    readonly NariNoteDbContext context;
    
    public LikeRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Like?> FindByUserAndArticleAsync(UserId userId, ArticleId articleId)
    {
        return await context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.ArticleId == articleId);
    }
    
    public async Task<Like> CreateAsync(Like like)
    {
        context.Likes.Add(like);
        await context.SaveChangesAsync();
        return like;
    }

    public async Task<Like?> FindByIdAsync(LikeId id)
    {
        return await context.Likes.FindAsync(id);
    }

    public async Task<Like> FindForceByIdAsync(LikeId id)
    {
        var like = await FindByIdAsync(id);
        if (like == null) throw new KeyNotFoundException($"ID: {id} のいいねが見つかりません");
        
        return like;
    }

    public async Task<Like> UpdateAsync(Like entity)
    {
        context.Likes.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(LikeId id)
    {
        var like = await context.Likes.FindAsync(id);
        if (like == null) return;

        context.Likes.Remove(like);
        await context.SaveChangesAsync();
    }

    public async Task<int> CountByArticleAsync(ArticleId articleId)
    {
        return await context.Likes.CountAsync(l => l.ArticleId == articleId);
    }

    public async Task<List<Article>> FindLikedArticlesByUserAsync(UserId userId)
    {
        var likes = await context.Likes
            .Where(l => l.UserId == userId)
            .Include(l => l.Article)
                .ThenInclude(a => a.Author)
            .Include(l => l.Article.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(l => l.Article.Likes)
            .OrderByDescending(l => l.CreatedAt)
            .AsSplitQuery()
            .ToListAsync();

        return likes.Where(l => l.Article != null)
                    .Select(l => l.Article)
                    .ToList()!;
    }

    public async Task<int> CountLikedArticlesByUserAsync(UserId userId)
    {
        return await context.Likes
            .Where(l => l.UserId == userId)
            .CountAsync();
    }
}
