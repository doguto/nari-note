using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);
    Task<User> GetByIdAsync(int id);
    
    /// <summary>
    /// メールアドレスでユーザーを検索（存在しない場合はnullを返す）
    /// </summary>
    Task<User?> FindByEmailAsync(string email);
    
    /// <summary>
    /// ユーザーを作成する
    /// </summary>
    Task<User> CreateAsync(User user);
}
