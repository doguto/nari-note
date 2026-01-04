using Microsoft.AspNetCore.Http;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Security;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly IUserRepository userRepository;
    readonly ISessionRepository sessionRepository;
    readonly IJwtHelper jwtHelper;
    readonly ICookieOptionsHelper cookieOptionsHelper;

    public SignUpService(
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

    public async Task<AuthResponse> ExecuteAsync(SignUpRequest request, HttpResponse response)
    {
        var existingUser = await userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null) throw new ArgumentException("このメールアドレスは既に使用されています");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
        };

        var createdUser = await userRepository.CreateAsync(user);

        var sessionKey = jwtHelper.GenerateSessionKey();
        var token = jwtHelper.GenerateToken(createdUser, sessionKey);

        var session = new Session
        {
            UserId = createdUser.Id,
            SessionKey = sessionKey,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtHelper.GetExpirationInHours()),
            CreatedAt = DateTime.UtcNow,
            User = createdUser
        };
        
        await sessionRepository.CreateAsync(session);
        
        // HttpOnly Cookieにトークンを設定
        var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(
            TimeSpan.FromHours(jwtHelper.GetExpirationInHours()));
        response.Cookies.Append("authToken", token, cookieOptions);
        
        return new AuthResponse
        {
            UserId = createdUser.Id
        };
    }
}
