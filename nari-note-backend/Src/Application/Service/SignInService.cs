using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class SignInService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly PasswordHashingService passwordHashingService;
    readonly JwtTokenService jwtTokenService;
    
    public SignInService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        PasswordHashingService passwordHashingService,
        JwtTokenService jwtTokenService)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.passwordHashingService = passwordHashingService;
        this.jwtTokenService = jwtTokenService;
    }
    
    public async Task<AuthResponse> ExecuteAsync(SignInRequest request)
    {
        // メールアドレスでユーザーを検索
        var user = await userRepository.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            throw new UnauthorizedException("メールアドレスまたはパスワードが正しくありません");
        }
        
        // パスワードを検証
        if (!passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("メールアドレスまたはパスワードが正しくありません");
        }
        
        // JWTトークンを生成
        var token = jwtTokenService.GenerateToken(user);
        
        // セッションを作成
        var sessionKey = jwtTokenService.GenerateSessionKey();
        var session = new Session
        {
            UserId = user.Id,
            SessionKey = sessionKey,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        
        await sessionRepository.CreateAsync(session);
        
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
