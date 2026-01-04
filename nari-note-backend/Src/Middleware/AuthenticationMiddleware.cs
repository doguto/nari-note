using System.Net;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Application.Security;

namespace NariNoteBackend.Middleware;

public class AuthenticationMiddleware
{
    readonly RequestDelegate next;
    
    public AuthenticationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }
    
    public async Task InvokeAsync(
        HttpContext context, 
        IJwtHelper jwtHelper, 
        ISessionRepository sessionRepository)
    {
        // 認証が不要なエンドポイントはスキップ
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/auth/") || 
            path.Contains("/health") ||
            context.Request.Method == "OPTIONS")
        {
            await next(context);
            return;
        }
        
        // Authorizationヘッダーからトークンを取得
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
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
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        var principal = jwtHelper.ValidateToken(token);
        if (principal == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
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
        
        var sessionKeyClaim = principal.FindFirst("sessionKey");
        if (sessionKeyClaim == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = new 
                { 
                    code = "UNAUTHORIZED",
                    message = "セッション情報が見つかりません",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            });
            return;
        }
        
        var session = await sessionRepository.FindBySessionKeyAsync(sessionKeyClaim.Value);
        if (session == null || session.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = new 
                { 
                    code = "UNAUTHORIZED",
                    message = "セッションが無効または期限切れです",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            });
            return;
        }
        
        // ユーザー情報をHttpContextに設定
        context.Items["User"] = session.User;
        context.Items["UserId"] = session.UserId;
        
        await next(context);
    }
}
