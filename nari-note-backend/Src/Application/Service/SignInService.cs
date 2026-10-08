using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;

namespace NariNoteBackend.Application.Service;

public class SignInService
{
    readonly ICookieOptionsHelper cookieOptionsHelper;
    readonly IJwtHelper jwtHelper;
    readonly IUserRepository userRepository;

    public SignInService(
        IUserRepository userRepository,
        IJwtHelper jwtHelper,
        ICookieOptionsHelper cookieOptionsHelper
    )
    {
        this.userRepository = userRepository;
        this.jwtHelper = jwtHelper;
        this.cookieOptionsHelper = cookieOptionsHelper;
    }

    public async Task<AuthResponse> ExecuteAsync(SignInRequest request, HttpResponse response)
    {
        // Name は一意のため、メールアドレス → ユーザー名の順で照合する
        var user = await userRepository.FindByEmailAsync(request.UsernameOrEmail)
                   ?? await userRepository.FindByNameAsync(request.UsernameOrEmail);
        if (user == null) throw new ArgumentException("ユーザー名またはメールアドレス、もしくはパスワードが正しくありません");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) throw new ArgumentException("ユーザー名またはメールアドレス、もしくはパスワードが正しくありません");

        var token = jwtHelper.GenerateToken(user.Id, user.Name);

        // HttpOnly Cookieにトークンを設定
        var cookieOptions = cookieOptionsHelper.CreateAuthCookieOptions(
            TimeSpan.FromHours(jwtHelper.GetExpiration())
        );
        response.Cookies.Append("authToken", token, cookieOptions);

        return new AuthResponse
        {
            UserId = user.Id
        };
    }
}
