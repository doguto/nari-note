using System.IdentityModel.Tokens.Jwt;
using System.Net;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Middleware;

public class JwtAuthenticationMiddleware
{
    readonly ILogger<JwtAuthenticationMiddleware> logger;
    readonly RequestDelegate next;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IJwtHelper jwtHelper)
    {
        var endpoint = context.GetEndpoint();

        // エンドポイントの属性を確認
        var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        var optionalAuth = endpoint?.Metadata.GetMetadata<OptionalAuthAttribute>() != null;
        var requireAuth = endpoint?.Metadata.GetMetadata<RequireAuthAttribute>() != null;

        // Cookieからトークンを取得
        var token = context.Request.Cookies["authToken"];

        // トークンが見つからない場合
        if (token.IsNullOrEmpty())
        {
            if (allowAnonymous || optionalAuth)
            {
                // 認証不要または任意認証の場合、認証なしで次のミドルウェアに進む
                await next(context);
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Unauthorized.AsInt();
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "UNAUTHORIZED",
                    message = "認証が必要です",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            });
            return;
        }

        // JWTトークンを検証
        var principal = jwtHelper.ValidateToken(token!);
        if (principal == null)
        {
            if (allowAnonymous || optionalAuth)
            {
                // 認証不要または任意認証の場合、トークンが無効でも次のミドルウェアに進む
                await next(context);
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Unauthorized.AsInt();
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "UNAUTHORIZED",
                    message = "無効なトークンです",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            });
            return;
        }

        // JWTトークンからUserIdを取得
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userIdValue))
        {
            if (allowAnonymous || optionalAuth)
            {
                // 認証不要または任意認証の場合、ユーザーID情報がなくても次のミドルウェアに進む
                await next(context);
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Unauthorized.AsInt();
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "UNAUTHORIZED",
                    message = "ユーザー情報が見つかりません",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            });
            return;
        }

        // ユーザーIDをHttpContext.Itemsに設定
        context.Items["UserId"] = UserId.From(userIdValue);

        await next(context);
    }
}
