using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Security;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Service;

public class SignInService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly IJwtHelper jwtHelper;
    
    public SignInService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IJwtHelper jwtHelper)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.jwtHelper = jwtHelper;
    }
    
    public async Task<AuthResponse> ExecuteAsync(SignInRequest request)
    {
        var user = await userRepository.FindByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null) throw new ArgumentException("ユーザー名またはパスワードが正しくありません");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) throw new ArgumentException("ユーザー名またはパスワードが正しくありません");

        var sessionKey = jwtHelper.GenerateSessionKey();
        var token = jwtHelper.GenerateToken(user, sessionKey);

        var session = new Session
        {
            UserId = user.Id,
            SessionKey = sessionKey,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtHelper.GetExpirationInHours()),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        
        await sessionRepository.CreateAsync(session);
        
        return new AuthResponse
        {
            UserId = user.Id
        };
    }
}
