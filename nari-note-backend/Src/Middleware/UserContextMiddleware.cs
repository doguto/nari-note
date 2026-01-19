using System.IdentityModel.Tokens.Jwt;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Middleware;

/// <summary>
/// ASP.NET Core認証の結果をHttpContext.Itemsに設定するミドルウェア
/// </summary>
public class UserContextMiddleware
{
    readonly RequestDelegate next;

    public UserContextMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 認証済みの場合、UserIdをHttpContext.Itemsに設定
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userIdValue))
            {
                context.Items["UserId"] = UserId.From(userIdValue);
            }
        }

        await next(context);
    }
}
