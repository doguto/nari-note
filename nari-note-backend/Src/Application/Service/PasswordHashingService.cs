namespace NariNoteBackend.Application.Service;

public class PasswordHashingService
{
    /// <summary>
    /// パスワードをハッシュ化する
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    /// <summary>
    /// パスワードを検証する
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
