using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    
    Task<Session?> FindBySessionKeyAsync(string sessionKey);
    
    Task<List<Session>> FindByUserIdAsync(int userId);
    
    Task DeleteAsync(int id);
    
    Task DeleteAllByUserIdAsync(int userId);
    
    Task DeleteExpiredSessionsAsync();
}
