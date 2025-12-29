using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;
using NariNoteBackend.Infrastructure.Helper;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly JwtHelper jwtHelper;
    
    public SignUpService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        JwtHelper jwtHelper)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.jwtHelper = jwtHelper;
    }
    
    public async Task<AuthResponse> ExecuteAsync(SignUpRequest request)
    {
        // 1. メールアドレスの重複チェック
        var existingUser = await userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException("このメールアドレスは既に使用されています");
        }
        
        // 2. パスワードをハッシュ化
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        // 3. ユーザーを作成
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var createdUser = await userRepository.CreateAsync(user);
        
        // 4. セッションキーを生成
        var sessionKey = JwtHelper.GenerateSessionKey();
        
        // 5. JWTトークンを生成
        var token = jwtHelper.GenerateToken(createdUser, sessionKey);
        
        // 6. セッションを作成
        var session = new Session
        {
            UserId = createdUser.Id,
            SessionKey = sessionKey,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtHelper.GetExpirationInHours()),
            CreatedAt = DateTime.UtcNow,
            User = createdUser
        };
        
        await sessionRepository.CreateAsync(session);
        
        // 7. レスポンスを返却
        return new AuthResponse
        {
            UserId = createdUser.Id,
            ExpiresAt = session.ExpiresAt
        };
    }
}
