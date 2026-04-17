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
}
