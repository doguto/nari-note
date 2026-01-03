# エラーハンドリング戦略

## 概要

nari-note-backendでは、**GlobalExceptionHandlerMiddleware**を使用して全ての例外を統一的に処理します。

- Controller層、Service層ではtry-catchは基本的に不要
- 適切な例外をthrowすれば、ミドルウェアが自動的に適切なHTTPステータスコードとレスポンスに変換
- Repository層でも基本的にtry-catchは不使用（特定の例外ハンドリングが必要な場合を除く）

## 例外とHTTPステータスコードのマッピング

### GlobalExceptionHandlerMiddlewareの実装

```csharp
using System.ComponentModel.DataAnnotations;
using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    readonly RequestDelegate next;
    readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger
    )
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            var response = BuildErrorResponse(ex);
            httpContext.Response.StatusCode = response.StatusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }

    ErrorResponse BuildErrorResponse(Exception ex)
    {
        var statusCode = ex switch
        {
            // 400 Bad Request
            ArgumentNullException or ArgumentOutOfRangeException or ArgumentException 
                or ValidationException or InvalidOperationException
                => HttpStatusCode.BadRequest,

            // 401 Unauthorized
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,

            // 404 Not Found
            KeyNotFoundException => HttpStatusCode.NotFound,

            // 408 Request Timeout
            TimeoutException => HttpStatusCode.RequestTimeout,

            // TODO: メンテナンス等によるサービスの停止中 503 ServiceUnavailable
            
            // 500 Internal Server Error
            // NariNoteException はカスタムのエラーであるため可読性の為 500 であることを明記
            NariNoteException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError,
        };

        return new ErrorResponse()
        {
            StatusCode = statusCode.AsInt(),
            Message = ex.Message,
        };
    }
}
```

### エラーレスポンス形式

```csharp
namespace NariNoteBackend.Application.Dto.Response;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

## 例外の種類とHTTPステータスコード

### 400 Bad Request

**使用する例外:**
- `ArgumentNullException`
- `ArgumentOutOfRangeException`
- `ArgumentException`
- `System.ComponentModel.DataAnnotations.ValidationException`
- `InvalidOperationException`

**用途:**
- バリデーションエラー
- 不正なリクエストパラメータ
- ビジネスルール違反

**フロントエンドの挙動:**
フォーム上にバリデーションエラーを表示する

**例:**
```csharp
if (string.IsNullOrWhiteSpace(request.Title))
{
    throw new ArgumentException("タイトルは必須です");
}

if (request.AuthorId <= 0)
{
    throw new ArgumentOutOfRangeException(nameof(request.AuthorId), "著者IDは1以上の値を指定してください");
}
```

### 401 Unauthorized

**使用する例外:**
- `UnauthorizedAccessException`

**用途:**
- 認証エラー（ログインしていない）
- 認可エラー（権限がない）

**フロントエンドの挙動:**
サインインページにリダイレクトする

**例:**
```csharp
// 記事の編集権限チェック
if (article.AuthorId != userId)
{
    throw new UnauthorizedAccessException("この記事を編集する権限がありません");
}
```

### 404 Not Found

**使用する例外:**
- `KeyNotFoundException`

**用途:**
- リソースが見つからない

**フロントエンドの挙動:**
404用のページにリダイレクトする

**例:**
```csharp
public async Task<Article> FindForceByIdAsync(int id)
{
    var article = await FindByIdAsync(id);
    if (article == null)
    {
        throw new KeyNotFoundException($"記事{id}が存在しません");
    }
    return article;
}
```

### 408 Request Timeout

**使用する例外:**
- `TimeoutException`

**用途:**
- リクエストタイムアウト
- 外部API呼び出しのタイムアウト

**フロントエンドの挙動:**
エラーメッセージを表示し、再試行を促す

### 500 Internal Server Error

**使用する例外:**
- `NariNoteException`（カスタム例外基底クラス）
- その他の例外（上記以外）

**用途:**
- 予期しないエラー
- システムエラー

**フロントエンドの挙動:**
アラートを出して操作前の状態に戻す

**NariNoteException:**
```csharp
namespace NariNoteBackend.Application.Exception;

public class NariNoteException : System.Exception
{
    
}
```

### 503 Service Unavailable

**TODO:** メンテナンス等によるサービスの停止中

**フロントエンドの挙動:**
メンテナンス等のページにリダイレクトする

## 実装例

### Controller層

```csharp
[HttpPost]
[ValidateModelState]
public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
{
    // try-catchは不要
    // 例外はGlobalExceptionHandlerMiddlewareが処理
    var response = await createArticleService.ExecuteAsync(request);
    return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
}
```

### Service層

```csharp
public async Task<UpdateArticleResponse> ExecuteAsync(int userId, UpdateArticleRequest request)
{
    // try-catchは不要
    // 適切な例外をthrowすればミドルウェアが処理
    var article = await articleRepository.FindForceByIdAsync(request.Id);
    
    if (article.AuthorId != userId)
    {
        throw new UnauthorizedAccessException("この記事を編集する権限がありません");
    }
    
    // 更新処理...
}
```

### Repository層

```csharp
public async Task<Article> FindForceByIdAsync(int id)
{
    var article = await FindByIdAsync(id);
    if (article == null)
    {
        // KeyNotFoundExceptionをthrow
        // ミドルウェアが404 Not Foundに変換
        throw new KeyNotFoundException($"記事{id}が存在しません");
    }
    return article;
}
```

## バリデーションエラーの処理

### ValidateModelStateAttributeによる自動処理

```csharp
[HttpPost]
[ValidateModelState]  // このフィルタが自動的にModelStateをチェック
public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
{
    // ModelStateは既にチェック済み
    var response = await createArticleService.ExecuteAsync(request);
    return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
}
```

### Request DTOでのバリデーション属性

```csharp
public class CreateArticleRequest
{
    [Required(ErrorMessage = "タイトルは必須です")]
    [MaxLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "本文は必須です")]
    public string Body { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "著者IDは必須です")]
    [Range(1, int.MaxValue, ErrorMessage = "著者IDは1以上の値を指定してください")]
    public int AuthorId { get; set; }
}
```

## まとめ

### 開発者が意識すること

1. **適切な例外をthrow**
   - 400系エラー: `ArgumentException`, `ValidationException`, `UnauthorizedAccessException`, `KeyNotFoundException`
   - 500系エラー: `NariNoteException` またはその他の例外

2. **try-catchは基本的に不要**
   - Controller層、Service層、Repository層すべてで不要
   - 特定の例外処理が必要な場合のみ使用

3. **バリデーションは属性で定義**
   - Request DTOに `[Required]`, `[MaxLength]`, `[Range]` 等を付与
   - `[ValidateModelState]` フィルタで自動チェック

4. **エラーメッセージは日本語で**
   - ユーザーに表示されるため、わかりやすい日本語で記述

