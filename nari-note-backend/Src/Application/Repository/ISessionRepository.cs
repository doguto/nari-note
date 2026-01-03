using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> FindBySessionKeyAsync(string sessionKey);
    Task<List<Session>> FindByUserIdAsync(int userId);
}
