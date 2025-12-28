using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IUserRepository
{
    /// <summary>
    /// ユーザーを作成する
    /// </summary>
    Task<User> CreateAsync(User user);
    
    /// <summary>
    /// IDでユーザーを検索（存在しない場合はnullを返す）
    /// </summary>
    Task<User?> FindByIdAsync(int id);
    
    /// <summary>
    /// IDでユーザーを取得（存在しない場合はNotFoundExceptionをthrow）
    /// </summary>
    Task<User> GetByIdAsync(int id);
    
    /// <summary>
    /// メールアドレスでユーザーを検索（存在しない場合はnullを返す）
    /// </summary>
    Task<User?> FindByEmailAsync(string email);
    
    /// <summary>
    /// メールアドレスでユーザーを取得（存在しない場合はNotFoundExceptionをthrow）
    /// </summary>
    Task<User> GetByEmailAsync(string email);
}
