using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Repository;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail);
}
