using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface ISessionRepository
{
    /// <summary>
    /// セッションを作成する
    /// </summary>
    Task<Session> CreateAsync(Session session);
    
    /// <summary>
    /// セッションキーでセッションを検索（存在しない場合はnullを返す）
    /// </summary>
    Task<Session?> FindBySessionKeyAsync(string sessionKey);
    
    /// <summary>
    /// ユーザーIDでセッション一覧を取得
    /// </summary>
    Task<List<Session>> FindByUserIdAsync(int userId);
    
    /// <summary>
    /// セッションを削除する
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// ユーザーの全セッションを削除する
    /// </summary>
    Task DeleteAllByUserIdAsync(int userId);
    
    /// <summary>
    /// 期限切れのセッションを削除する
    /// </summary>
    Task DeleteExpiredSessionsAsync();
}
