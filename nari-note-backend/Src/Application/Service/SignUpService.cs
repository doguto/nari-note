using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly PasswordHashingService passwordHashingService;
    readonly JwtTokenService jwtTokenService;
    
    public SignUpService(
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
    
    public async Task<AuthResponse> ExecuteAsync(SignUpRequest request)
    {
        // メールアドレスの重複チェック
        var existingUser = await userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException("このメールアドレスは既に使用されています");
        }
        
        // パスワードをハッシュ化
        var passwordHash = passwordHashingService.HashPassword(request.Password);
        
        // ユーザーを作成
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var createdUser = await userRepository.CreateAsync(user);
        
        // JWTトークンを生成
        var token = jwtTokenService.GenerateToken(createdUser);
        
        // セッションを作成
        var sessionKey = jwtTokenService.GenerateSessionKey();
        var session = new Session
        {
            UserId = createdUser.Id,
            SessionKey = sessionKey,
            ExpiresAt = jwtTokenService.GetSessionExpiration(),
            CreatedAt = DateTime.UtcNow,
            User = createdUser
        };
        
        await sessionRepository.CreateAsync(session);
        
        return new AuthResponse
        {
            Token = token,
            UserId = createdUser.Id,
            Email = createdUser.Email,
            Name = createdUser.Name,
            ExpiresAt = session.ExpiresAt
        };
    }
}
