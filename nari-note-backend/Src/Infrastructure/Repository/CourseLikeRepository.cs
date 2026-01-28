using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class CourseLikeRepository : ICourseLikeRepository
{
    readonly NariNoteDbContext context;
    
    public CourseLikeRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<CourseLike?> FindByIdAsync(CourseLikeId id)
    {
        return await context.CourseLikes.FindAsync(id);
    }

    public async Task<CourseLike> FindForceByIdAsync(CourseLikeId id)
    {
        var courseLike = await FindByIdAsync(id);
        if (courseLike == null) throw new KeyNotFoundException($"ID: {id} の講座いいねが見つかりません");
        
        return courseLike;
    }

    public async Task<CourseLike> CreateAsync(CourseLike entity)
    {
        context.CourseLikes.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<CourseLike> UpdateAsync(CourseLike entity)
    {
        context.CourseLikes.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(CourseLikeId id)
    {
        await context.CourseLikes
            .Where(cl => cl.Id == id)
            .ExecuteDeleteAsync();
    }
}
