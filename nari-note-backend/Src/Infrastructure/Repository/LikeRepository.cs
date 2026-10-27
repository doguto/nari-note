using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
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

    public async Task<(List<ArticleSummary> Articles, int TotalCount)> FindLikedArticleSummariesByUserAsync(UserId userId, int limit, int offset)
    {
        var query = context.Likes.Where(l => l.UserId == userId);

        // いいねした日時の新しい順に並べる（Likes の (UserId, CreatedAt) インデックスを使う）
        var totalCount = await query.CountAsync();
        var articles = await query.OrderByDescending(l => l.CreatedAt)
                                  .ThenByDescending(l => l.Id)
                                  .Skip(offset)
                                  .Take(limit)
                                  .Select(l => l.Article)
                                  .Select(ArticleSummaryProjection.FromArticle)
                                  .ToListAsync();

        return (articles, totalCount);
    }

    public async Task<int> CountLikedArticlesByUserAsync(UserId userId)
    {
        return await context.Likes
            .Where(l => l.UserId == userId)
            .CountAsync();
    }
}
