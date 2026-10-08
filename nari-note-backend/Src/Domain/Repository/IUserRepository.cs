using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, UserId? excludeUserId = null);
}
