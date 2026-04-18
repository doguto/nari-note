using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class KifuRepository : IKifuRepository
{
    readonly NariNoteDbContext context;

    public KifuRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<Kifu> CreateAsync(Kifu entity)
    {
        context.Kifus.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Kifu?> FindByIdAsync(KifuId id)
    {
        return await context.Kifus.FindAsync(id);
    }

    public async Task<Kifu> FindForceByIdAsync(KifuId id)
    {
        var kifu = await FindByIdAsync(id);
        if (kifu == null) throw new KeyNotFoundException($"棋譜{id}が存在しません");
        return kifu;
    }

    public async Task<Kifu> UpdateAsync(Kifu entity)
    {
        context.Kifus.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(KifuId id)
    {
        var kifu = await FindByIdAsync(id);
        if (kifu == null) return;

        context.Kifus.Remove(kifu);
        await context.SaveChangesAsync();
    }

    public async Task ReplaceAllByArticleIdAsync(ArticleId articleId, List<Kifu> kifus)
    {
        var existing = await context.Kifus.Where(k => k.ArticleId == articleId).ToListAsync();

        if (existing.Count >= kifus.Count)
        {
            var deleteCount = existing.Count - kifus.Count;
            var updateKifus = kifus.Take(kifus.Count).ToArray();
            var deleteKifus = kifus.Skip(kifus.Count).Take(deleteCount);

            for (var i = 0; i < kifus.Count; i++)
            {
                updateKifus[i] = existing[i];
                updateKifus[i].KifuText = kifus[i].KifuText;
                updateKifus[i].SortOrder = kifus[i].SortOrder;
            }

            context.Kifus.UpdateRange(updateKifus);
            context.Kifus.RemoveRange(deleteKifus);
        }
        else if (existing.Count < kifus.Count)
        {
            var insertCount = kifus.Count - existing.Count;
            var insertKifus = kifus.Skip(existing.Count).Take(insertCount);

            for (var i = 0; i < existing.Count; i++)
            {
                existing[i].KifuText = kifus[i].KifuText;
                existing[i].SortOrder = kifus[i].SortOrder;
            }

            context.Kifus.UpdateRange(existing);
            context.Kifus.AddRange(insertKifus);
        }

        await context.SaveChangesAsync();
    }
}
