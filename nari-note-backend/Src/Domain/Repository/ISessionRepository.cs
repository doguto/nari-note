using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ISessionRepository : IRepository<Session, SessionId>
{
    Task<Session?> FindBySessionKeyAsync(string sessionKey);
    Task<List<Session>> FindByUserIdAsync(UserId userId);
}
