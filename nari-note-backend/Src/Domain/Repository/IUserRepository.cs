using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> ExistsByNameAsync(string name, UserId? excludeUserId = null);

    // createdBefore より前に作成され、有効な確認トークンを持たないメール未認証ユーザーを削除し、削除件数を返す
    Task<int> DeleteStaleUnverifiedAsync(DateTime createdBefore, DateTime now);
}
