using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Infrastructure.Authentication;

/// <summary>
/// JWT認証ハンドラー（Cookie、Authorizationヘッダーの両方に対応）
/// </summary>
public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    readonly IJwtHelper jwtHelper;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtHelper jwtHelper
    ) : base(options, logger, encoder)
    {
        this.jwtHelper = jwtHelper;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Cookieからトークンを取得（優先）
        var token = Context.Request.Cookies["authToken"];

        // Cookieにトークンがない場合、Authorizationヘッダーから取得
        if (token.IsNullOrEmpty())
        {
            var authHeader = Context.Request.Headers["Authorization"].ToString();
            if (!authHeader.IsNullOrEmpty() && authHeader.StartsWith("Bearer "))
                token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // トークンが見つからない場合
        if (token.IsNullOrEmpty())
        {
            return AuthenticateResult.NoResult();
        }

        var principal = jwtHelper.ValidateToken(token!);
        if (principal == null)
        {
            return AuthenticateResult.Fail("無効なトークンです");
        }

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
