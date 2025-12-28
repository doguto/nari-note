using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

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
        try
        {
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            return session;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505") // Unique constraint violation
            {
                throw new ConflictException("Session key already exists", ex);
            }
            if (pgEx.SqlState == "23503") // Foreign key violation
            {
                throw new ValidationException("Invalid user reference", null, ex);
            }
            throw new InfrastructureException(
                "Database error occurred while creating session", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The session was modified by another user", ex);
        }
    }
    
    public async Task<Session?> FindBySessionKeyAsync(string sessionKey)
    {
        return await context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.ExpiresAt > DateTime.UtcNow);
    }
    
    public async Task<Session> GetBySessionKeyAsync(string sessionKey)
    {
        var session = await FindBySessionKeyAsync(sessionKey);
        
        if (session == null)
        {
            throw new UnauthorizedException("Invalid or expired session");
        }
        
        return session;
    }
    
    public async Task<List<Session>> FindActiveSessionsByUserIdAsync(int userId)
    {
        return await context.Sessions
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var session = await context.Sessions.FindAsync(id);
            if (session != null)
            {
                context.Sessions.Remove(session);
                await context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException(
                "The session was modified or deleted by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                $"Database error occurred while deleting session with ID {id}", ex);
        }
    }
    
    public async Task DeleteAllByUserIdAsync(int userId)
    {
        try
        {
            var sessions = await context.Sessions
                .Where(s => s.UserId == userId)
                .ToListAsync();
            
            context.Sessions.RemoveRange(sessions);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                $"Database error occurred while deleting sessions for user {userId}", ex);
        }
    }
    
    public async Task DeleteExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await context.Sessions
                .Where(s => s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            
            context.Sessions.RemoveRange(expiredSessions);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                "Database error occurred while deleting expired sessions", ex);
        }
    }
}
