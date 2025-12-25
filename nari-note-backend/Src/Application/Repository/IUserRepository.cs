using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);
}
