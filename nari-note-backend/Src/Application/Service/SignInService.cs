using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;
using NariNoteBackend.Infrastructure.Helper;

namespace NariNoteBackend.Application.Service;

public class SignInService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly JwtHelper jwtHelper;
    
    public SignInService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        JwtHelper jwtHelper)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.jwtHelper = jwtHelper;
    }
    
    public async Task<AuthResponse> ExecuteAsync(SignInRequest request)
    {
        // 1. メールアドレスでユーザーを検索
        var user = await userRepository.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("メールアドレスまたはパスワードが正しくありません");
        }
        
        // 2. パスワードを検証
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("メールアドレスまたはパスワードが正しくありません");
        }
        
        // 3. セッションキーを生成
        var sessionKey = JwtHelper.GenerateSessionKey();
        
        // 4. JWTトークンを生成
        var token = jwtHelper.GenerateToken(user, sessionKey);
        
        // 5. セッションを作成
        var session = new Session
        {
            UserId = user.Id,
            SessionKey = sessionKey,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        
        await sessionRepository.CreateAsync(session);
        
        // 6. レスポンスを返却
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = session.ExpiresAt
        };
    }
}
