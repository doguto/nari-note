using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class FollowRepository : IFollowRepository
{
    readonly NariNoteDbContext context;
    
    public FollowRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Follow?> FindByFollowerAndFollowingAsync(UserId followerId, UserId followingId)
    {
        return await context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }
    
    public async Task<Follow> CreateAsync(Follow follow)
    {
        context.Follows.Add(follow);
        await context.SaveChangesAsync();
        return follow;
    }

    public async Task<Follow?> FindByIdAsync(FollowId id)
    {
        return await context.Follows.FindAsync(id);
    }

    public async Task<Follow> FindForceByIdAsync(FollowId id)
    {
        var follow = await FindByIdAsync(id);
        if (follow == null) throw new KeyNotFoundException($"ID: {id} のフォローが見つかりません");
        
        return follow;
    }

    public async Task<Follow> UpdateAsync(Follow entity)
    {
        context.Follows.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(FollowId id)
    {
        var follow = await context.Follows.FindAsync(id);
        if (follow == null) return;

        context.Follows.Remove(follow);
        await context.SaveChangesAsync();
    }

    public async Task<int> CountFollowersAsync(UserId userId)
    {
        return await context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> CountFollowingsAsync(UserId userId)
    {
        return await context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    public async Task<(List<User> Users, int TotalCount)> GetFollowersAsync(UserId userId, int limit, int offset)
    {
        var query = context.Follows.Where(f => f.FollowingId == userId);

        // フォローされた日時の新しい順に並べる
        var totalCount = await query.CountAsync();
        var followers = await query.OrderByDescending(f => f.CreatedAt)
                                   .ThenByDescending(f => f.Id)
                                   .Skip(offset)
                                   .Take(limit)
                                   .Select(f => f.Follower)
                                   .ToListAsync();

        return (followers, totalCount);
    }

    public async Task<(List<User> Users, int TotalCount)> GetFollowingsAsync(UserId userId, int limit, int offset)
    {
        var query = context.Follows.Where(f => f.FollowerId == userId);

        // フォローした日時の新しい順に並べる
        var totalCount = await query.CountAsync();
        var followings = await query.OrderByDescending(f => f.CreatedAt)
                                    .ThenByDescending(f => f.Id)
                                    .Skip(offset)
                                    .Take(limit)
                                    .Select(f => f.Following)
                                    .ToListAsync();

        return (followings, totalCount);
    }
}
