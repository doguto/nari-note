using System.Net;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Application.Security;
using NariNoteBackend.Extension;

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
        var method = context.Request.Method;
        
        // 認証不要のエンドポイントリスト（ホワイトリスト方式）
        var publicEndpoints = new[]
        {
            ("/auth/signin", "POST"),
            ("/auth/signup", "POST"),
            ("/health", "GET"),
            ("/api/articles", "GET"),          // 記事一覧の取得
            ("/api/users", "GET")              // ユーザー一覧の取得（プロフィール表示用）
        };
        
        // パスパターンマッチング: /api/articles/{id} のような動的パスに対応
        var isPublicEndpoint = publicEndpoints.Any(endpoint =>
        {
            var (endpointPath, endpointMethod) = endpoint;
            
            // メソッドが一致しない場合はスキップ
            if (method != endpointMethod && endpointMethod != "OPTIONS")
                return false;
            
            // 完全一致
            if (path == endpointPath)
                return true;
            
            // 動的パス対応: /api/articles/{id} パターン
            if (endpointPath == "/api/articles" && path.StartsWith("/api/articles/") && method == "GET")
                return true;
            
            // 動的パス対応: /api/users/{id} パターン（プロフィール取得のみ）
            if (endpointPath == "/api/users" && path.StartsWith("/api/users/") && method == "GET")
                return true;
            
            return false;
        });
        
        // OPTIONSリクエストは常に許可
        if (method == "OPTIONS" || isPublicEndpoint)
        {
            await next(context);
            return;
        }
        
        // Cookieからトークンを取得（優先）
        var token = context.Request.Cookies["authToken"];
        
        // Cookieにトークンがない場合、Authorizationヘッダーから取得
        if (token.IsNullOrEmpty())
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!authHeader.IsNullOrEmpty() && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        
        // トークンが見つからない場合
        if (token.IsNullOrEmpty())
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
        
        var principal = jwtHelper.ValidateToken(token!);
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
