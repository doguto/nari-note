using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class TagRepository : ITagRepository
{
    readonly NariNoteDbContext context;
    readonly TimeProvider timeProvider;

    public TagRepository(NariNoteDbContext context, TimeProvider timeProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public async Task<Tag> CreateAsync(Tag entity)
    {
        context.Tags.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Tag?> FindByIdAsync(TagId id)
    {
        return await context.Tags
                            .Include(t => t.ArticleTags)
                            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tag> FindForceByIdAsync(TagId id)
    {
        var tag = await FindByIdAsync(id);
        if (tag == null) throw new KeyNotFoundException($"タグ{id}が存在しません");

        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag entity)
    {
        context.Tags.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(TagId id)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Tag>> GetPopularTagsAsync(DateTime sinceDate, int limit)
    {
        // 公開日時を過ぎていない予約投稿は集計に含めない
        var now = timeProvider.UtcNow();

        return await context.Tags
            .AsNoTracking()
            .Include(t => t.ArticleTags.Where(at => 
                at.Article.PublishedAt.HasValue && 
                at.Article.PublishedAt.Value >= sinceDate &&
                at.Article.PublishedAt.Value <= now))
            .Where(t => t.ArticleTags.Any(at => 
                at.Article.PublishedAt.HasValue && 
                at.Article.PublishedAt.Value >= sinceDate &&
                at.Article.PublishedAt.Value <= now))
            .OrderByDescending(t => t.ArticleTags.Count(at => 
                at.Article.PublishedAt.HasValue && 
                at.Article.PublishedAt.Value >= sinceDate &&
                at.Article.PublishedAt.Value <= now))
            .Take(limit)
            .ToListAsync();
    }
}
