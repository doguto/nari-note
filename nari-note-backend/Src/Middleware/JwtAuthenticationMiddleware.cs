using System.Net;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
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

    public async Task InvokeAsync(HttpContext context, IJwtHelper jwtHelper, IUserRepository userRepository)
    {
        var endpoint = context.GetEndpoint();

        // エンドポイントの属性を確認
        var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        var optionalAuth = endpoint?.Metadata.GetMetadata<OptionalAuthAttribute>() != null;

        var token = context.Request.Cookies["authToken"];

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

        var userId = jwtHelper.GetUserIdFromToken(token!);
        var userName = jwtHelper.GetUserNameFromToken(token!);
        if (userId == null || userName == null)
        {
            if (allowAnonymous || optionalAuth)
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Unauthorized.AsInt();
            await context.Response.WriteAsJsonAsync(
                new
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

        // 退会済みユーザーのトークンは無効として扱う
        var user = await userRepository.FindByIdAsync(userId.Value);
        if (user == null)
        {
            context.Response.Cookies.Delete("authToken", new CookieOptions
            {
                Path = "/"
            });

            if (allowAnonymous || optionalAuth)
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Unauthorized.AsInt();
            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = new
                    {
                        code = "UNAUTHORIZED",
                        message = "ユーザーが存在しません",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path
                    }
                });
            return;
        }

        context.Items["UserId"] = userId;
        context.Items["UserName"] = userName;

        await next(context);
    }
}
