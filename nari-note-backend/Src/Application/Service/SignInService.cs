using Microsoft.AspNetCore.Http;
using NariNoteBackend.Domain.Repository;
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
    readonly ICookieOptionsHelper cookieOptionsHelper;
    
    public SignInService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IJwtHelper jwtHelper,
        ICookieOptionsHelper cookieOptionsHelper)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.jwtHelper = jwtHelper;
        this.cookieOptionsHelper = cookieOptionsHelper;
    }
    
    public async Task<AuthResponse> ExecuteAsync(SignInRequest request, HttpResponse response)
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
        
        // HttpOnly Cookieにトークンを設定
        var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(
            TimeSpan.FromHours(jwtHelper.GetExpirationInHours()));
        response.Cookies.Append("authToken", token, cookieOptions);
        
        return new AuthResponse
        {
            UserId = user.Id
        };
    }
}
