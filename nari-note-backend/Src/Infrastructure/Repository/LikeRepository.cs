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
        var likedArticles = await context.Likes
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new { l.ArticleId, l.CreatedAt })
            .ToListAsync();

        if (!likedArticles.Any())
        {
            return new List<Article>();
        }

        var articleIds = likedArticles.Select(la => la.ArticleId).ToList();

        var articles = await context.Articles
            .Where(a => articleIds.Contains(a.Id))
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .ToListAsync();

        // Preserve the original liked order
        var orderedArticles = likedArticles
            .Select(la => articles.FirstOrDefault(a => a.Id == la.ArticleId))
            .Where(a => a != null)
            .ToList();

        return orderedArticles!;
    }
}
