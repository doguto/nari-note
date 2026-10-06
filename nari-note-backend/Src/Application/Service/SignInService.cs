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
        // サインインはメールアドレスのみで照合する（Name は一意でないため対象外）
        var user = await userRepository.FindByEmailAsync(request.UsernameOrEmail);
        if (user == null) throw new ArgumentException("メールアドレスまたはパスワードが正しくありません");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) throw new ArgumentException("メールアドレスまたはパスワードが正しくありません");

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
