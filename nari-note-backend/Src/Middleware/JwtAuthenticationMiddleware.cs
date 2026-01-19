using System.IdentityModel.Tokens.Jwt;
using System.Net;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Middleware;

/// <summary>
/// JWT認証ミドルウェア
/// CookieまたはAuthorizationヘッダーからJWTトークンを取得し、
/// 検証後にUserIdをHttpContext.Itemsに設定する
/// </summary>
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
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method;

        // OPTIONSリクエストは常に許可
        if (method == "OPTIONS" || IsPublicEndpoint(method, path))
        {
            await next(context);
            return;
        }

        // 任意認証エンドポイントの場合、認証失敗時でもリクエストを通す
        var isOptionalAuth = IsOptionalAuthEndpoint(method, path);

        // Cookieからトークンを取得（優先）
        var token = context.Request.Cookies["authToken"];

        // Cookieにトークンがない場合、Authorizationヘッダーから取得
        if (token.IsNullOrEmpty())
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!authHeader.IsNullOrEmpty() && authHeader.StartsWith("Bearer "))
                token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // トークンが見つからない場合
        if (token.IsNullOrEmpty())
        {
            if (isOptionalAuth)
            {
                // 任意認証の場合、認証なしで次のミドルウェアに進む
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
            if (isOptionalAuth)
            {
                // 任意認証の場合、トークンが無効でも次のミドルウェアに進む
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
            if (isOptionalAuth)
            {
                // 任意認証の場合、ユーザーID情報がなくても次のミドルウェアに進む
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

    bool IsPublicEndpoint(string method, string path)
    {
        // 認証不要のエンドポイントリスト（ホワイトリスト方式）
        var publicEndpoints = new[]
        {
            ("/api/auth/signin", HttpMethod.Post.ToString()),
            ("/api/auth/signup", HttpMethod.Post.ToString()),
            ("/api/health", HttpMethod.Get.ToString()),
            ("/api/articles", HttpMethod.Get.ToString())
        };

        // パスパターンマッチング: /api/articles/{id} のような動的パスに対応
        return publicEndpoints.Any(endpoint =>
        {
            var (endpointPath, endpointMethod) = endpoint;

            // メソッドが一致しない場合はスキップ
            if (method != endpointMethod && endpointMethod != "OPTIONS") return false;

            // 完全一致
            if (path == endpointPath) return true;

            // 動的パス対応: /api/articles/{id} パターン
            // ただし /api/articles/drafts は認証必須なので除外
            if (endpointPath == "/api/articles" && path.StartsWith("/api/articles/") 
                && path != "/api/articles/drafts" 
                && !path.Contains("/like")
                && !path.Contains("/comments")
                && method == "GET") return true;

            return false;
        });
    }

    bool IsOptionalAuthEndpoint(string method, string path)
    {
        // 任意認証のエンドポイントリスト（認証があってもなくてもよいエンドポイント）
        var optionalAuthEndpoints = new[]
        {
            ("/api/users", HttpMethod.Get.ToString()),  // ユーザー一覧は任意認証
        };

        return optionalAuthEndpoints.Any(endpoint =>
        {
            var (endpointPath, endpointMethod) = endpoint;

            // メソッドが一致しない場合はスキップ
            if (method != endpointMethod) return false;

            // 完全一致
            if (path == endpointPath) return true;

            // 動的パス対応: /api/users/{id} パターン（プロフィール取得）
            if (endpointPath == "/api/users" && path.StartsWith("/api/users/") && method == "GET") return true;

            return false;
        });
    }
}
