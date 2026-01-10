using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Repository;

public class SessionRepository : ISessionRepository
{
    readonly NariNoteDbContext context;
    
    public SessionRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Session> CreateAsync(Session session)
    {
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<Session?> FindByIdAsync(SessionId id)
    {
        return await context.Sessions.FindAsync(id);
    }

    public async Task<Session> FindForceByIdAsync(SessionId id)
    {
        var session = await FindByIdAsync(id);
        if (session == null) throw new KeyNotFoundException($"ID: {id} のセッションが見つかりません");
        return session;
    }

    public async Task<Session> UpdateAsync(Session entity)
    {
        context.Sessions.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Session?> FindBySessionKeyAsync(string sessionKey)
    {
        return await context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionKey == sessionKey);
    }
    
    public async Task<List<Session>> FindByUserIdAsync(UserId userId)
    {
        return await context.Sessions
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(SessionId id)
    {
        var session = await context.Sessions.FindAsync(id);
        if (session != null)
        {
            context.Sessions.Remove(session);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllByUserIdAsync(UserId userId)
    {
        var sessions = await context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();
        
        context.Sessions.RemoveRange(sessions);
        await context.SaveChangesAsync();
    }

    public async Task DeleteExpiredSessionsAsync()
    {
        var expiredSessions = await context.Sessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
        
        context.Sessions.RemoveRange(expiredSessions);
        await context.SaveChangesAsync();
    }
}
