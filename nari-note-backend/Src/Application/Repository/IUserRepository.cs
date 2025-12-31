using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);
    Task<User> GetByIdAsync(int id);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User> CreateAsync(User user);
}
