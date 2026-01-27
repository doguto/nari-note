using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class TagRepository : ITagRepository
{
    readonly NariNoteDbContext context;

    public TagRepository(NariNoteDbContext context)
    {
        this.context = context;
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

    public async Task<List<Tag>> GetAllWithArticleCountAsync()
    {
        return await context.Tags
                            .Include(t => t.ArticleTags)
                            .Where(t => t.ArticleTags.Any())
                            .OrderByDescending(t => t.CreatedAt)
                            .ToListAsync();
    }
}
