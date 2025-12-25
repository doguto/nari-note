# エラーハンドリング戦略

## 基本方針

### 前提条件

* APIは公開しない（内部利用のみ）
* HTTPレスポンスコードの正確性よりも、扱いやすさを優先
* Sentry等のエラー監視ツールとの連携を考慮
* すべてのエラーを適切にハンドリング（投げっぱなしにしない）
* 実装の度にcatchを記述する手間を削減

### 設計方針

1. **単一のカスタム例外基底クラス**を使用
   - すべてのアプリケーション例外は `ApplicationException` を継承
   - グローバルハンドラーで一括キャッチ可能

2. **グローバル例外ハンドラーミドルウェア**で統一的に処理
   - Controller、Service、Repositoryで発生した例外を一箇所でキャッチ
   - 各層で個別にtry-catchを書く必要なし

3. **エラー監視ツール連携**を考慮した設計
   - 例外情報を構造化してログ出力
   - Sentryなどの監視ツールで自動収集可能

4. **適切なHTTPステータスコード**の返却
   - 例外の種類に応じて適切なステータスコードを自動設定
   - クライアントが適切にエラーハンドリング可能

## アーキテクチャ

### エラーフロー

```
Repository/Service Layer
    ↓ throw ApplicationException（具体的な例外）
Global Exception Handler Middleware
    ↓ catch & convert
HTTP Response（統一されたエラー形式）
    ↓
Error Monitoring Tool (Sentry)
```

### レイヤー別の責務

#### Domain/Application Layer
* ビジネスロジックに基づく例外をthrow
* `ApplicationException`を継承したカスタム例外を使用
* try-catchは基本的に不要

```csharp
// 例：記事が見つからない場合
public async Task<Article> ExecuteAsync(int id)
{
    var article = await _repository.FindByIdAsync(id);
    if (article == null)
    {
        throw new NotFoundException($"Article with ID {id} not found");
    }
    return article;
}
```

#### Infrastructure Layer
* データアクセス時の例外を適切な`ApplicationException`に変換
* DBの制約違反などをビジネス例外に変換

```csharp
public async Task<Article> CreateAsync(Article article)
{
    try
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
    {
        if (pgEx.SqlState == "23505") // Unique constraint violation
        {
            throw new ConflictException("Article with this title already exists");
        }
        throw new InfrastructureException("Database error occurred", ex);
    }
}
```

#### Controller Layer
* 基本的にtry-catchは不要（ミドルウェアが処理）
* 必要に応じてバリデーションエラーを返却

```csharp
[HttpGet("{id}")]
public async Task<ActionResult> GetArticle(int id)
{
    // 例外はミドルウェアがキャッチするので、try-catchは不要
    var article = await _getArticleService.ExecuteAsync(id);
    return Ok(article);
}
```

#### Global Exception Handler Middleware
* すべての例外を一括キャッチ
* 例外の種類に応じて適切なHTTPレスポンスを生成
* エラーログを出力（監視ツール連携）

## 例外クラス設計

### 基底クラス：ApplicationException

```csharp
public abstract class ApplicationException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public Dictionary<string, object>? AdditionalData { get; }

    protected ApplicationException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null,
        Dictionary<string, object>? additionalData = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        AdditionalData = additionalData;
    }
}
```

### 具体的な例外クラス

#### NotFoundException (404)
リソースが見つからない場合

```csharp
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message, Exception? innerException = null)
        : base(message, 404, "NOT_FOUND", innerException)
    {
    }
}
```

#### ValidationException (400)
入力値が不正な場合

```csharp
public class ValidationException : ApplicationException
{
    public ValidationException(
        string message,
        Dictionary<string, object>? validationErrors = null,
        Exception? innerException = null)
        : base(message, 400, "VALIDATION_ERROR", innerException, validationErrors)
    {
    }
}
```

#### ConflictException (409)
リソースの競合が発生した場合

```csharp
public class ConflictException : ApplicationException
{
    public ConflictException(string message, Exception? innerException = null)
        : base(message, 409, "CONFLICT", innerException)
    {
    }
}
```

#### UnauthorizedException (401)
認証が必要な場合

```csharp
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message, Exception? innerException = null)
        : base(message, 401, "UNAUTHORIZED", innerException)
    {
    }
}
```

#### ForbiddenException (403)
権限が不足している場合

```csharp
public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message, Exception? innerException = null)
        : base(message, 403, "FORBIDDEN", innerException)
    {
    }
}
```

#### InfrastructureException (500)
インフラ層でのエラー（DB、外部API等）

```csharp
public class InfrastructureException : ApplicationException
{
    public InfrastructureException(string message, Exception? innerException = null)
        : base(message, 500, "INFRASTRUCTURE_ERROR", innerException)
    {
    }
}
```

## エラーレスポンス形式

### 統一されたエラーレスポンス

```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Article with ID 123 not found",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/123",
    "additionalData": {
      "articleId": 123
    }
  }
}
```

### エラーレスポンスクラス

```csharp
public class ErrorResponse
{
    public ErrorDetail Error { get; set; } = default!;
}

public class ErrorDetail
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = default!;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
```

## グローバル例外ハンドラーミドルウェア

```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationException ex)
        {
            await HandleApplicationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private async Task HandleApplicationExceptionAsync(
        HttpContext context,
        ApplicationException exception)
    {
        _logger.LogWarning(
            exception,
            "Application exception occurred: {ErrorCode} - {Message}",
            exception.ErrorCode,
            exception.Message);

        var response = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = exception.ErrorCode,
                Message = exception.Message,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                AdditionalData = exception.AdditionalData
            }
        };

        context.Response.StatusCode = exception.StatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        _logger.LogError(
            exception,
            "Unexpected exception occurred: {Message}",
            exception.Message);

        var response = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = "INTERNAL_SERVER_ERROR",
                Message = "An unexpected error occurred",
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            }
        };

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Program.csでの登録

```csharp
var builder = WebApplication.CreateBuilder(args);

// Services registration...

var app = builder.Build();

// グローバル例外ハンドラーを最初に登録（重要）
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Other middlewares...
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

## Sentry連携

### パッケージのインストール

```bash
dotnet add package Sentry.AspNetCore
```

### Program.csでの設定

```csharp
var builder = WebApplication.CreateBuilder(args);

// Sentry設定
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.TracesSampleRate = 1.0;
    options.Environment = builder.Environment.EnvironmentName;
});

// Services registration...

var app = builder.Build();

// Sentryミドルウェアを最初に登録
app.UseSentryTracing();

// グローバル例外ハンドラー
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Other middlewares...
app.Run();
```

### カスタム例外情報のSentryへの送信

```csharp
private async Task HandleApplicationExceptionAsync(
    HttpContext context,
    ApplicationException exception)
{
    // Sentryにカスタムデータを追加
    SentrySdk.ConfigureScope(scope =>
    {
        scope.SetTag("error_code", exception.ErrorCode);
        scope.SetExtra("status_code", exception.StatusCode);
        if (exception.AdditionalData != null)
        {
            foreach (var (key, value) in exception.AdditionalData)
            {
                scope.SetExtra(key, value);
            }
        }
    });

    // Sentryにキャプチャ
    SentrySdk.CaptureException(exception);

    // ログ出力
    _logger.LogWarning(
        exception,
        "Application exception occurred: {ErrorCode} - {Message}",
        exception.ErrorCode,
        exception.Message);

    // レスポンス生成...
}
```

## 実装ガイドライン

### Serviceでのエラーハンドリング

```csharp
public class UpdateArticleService
{
    private readonly IArticleRepository _repository;

    public async Task<Article> ExecuteAsync(int id, UpdateArticleRequest request)
    {
        // リソースの存在チェック
        var article = await _repository.FindByIdAsync(id);
        if (article == null)
        {
            throw new NotFoundException($"Article with ID {id} not found");
        }

        // 権限チェック
        if (article.AuthorId != request.UserId)
        {
            throw new ForbiddenException("You don't have permission to update this article");
        }

        // ビジネスロジック実行
        article.Title = request.Title;
        article.Body = request.Body;
        article.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(article);
        return article;
    }
}
```

### Repositoryでのエラーハンドリング

```csharp
public class ArticleRepository : IArticleRepository
{
    private readonly NariNoteDbContext _context;

    public async Task<Article> CreateAsync(Article article)
    {
        try
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            // DB制約違反を適切な例外に変換
            if (pgEx.SqlState == "23505") // Unique violation
            {
                throw new ConflictException(
                    "Article with this title already exists",
                    ex);
            }
            if (pgEx.SqlState == "23503") // Foreign key violation
            {
                throw new ValidationException(
                    "Invalid reference to related entity",
                    null,
                    ex);
            }
            // その他のDB例外
            throw new InfrastructureException(
                "Database error occurred while creating article",
                ex);
        }
    }

    public async Task<Article?> FindByIdAsync(int id)
    {
        try
        {
            return await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(
                $"Error occurred while fetching article with ID {id}",
                ex);
        }
    }
}
```

### Controllerでのエラーハンドリング

```csharp
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly GetArticleService _getArticleService;
    private readonly UpdateArticleService _updateArticleService;

    // try-catchは不要！ミドルウェアが処理
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(int id)
    {
        var article = await _getArticleService.ExecuteAsync(id);
        return Ok(article);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateArticle(
        int id,
        [FromBody] UpdateArticleRequest request)
    {
        // バリデーションエラーは既存のModelStateで処理
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // ビジネスロジックの例外はミドルウェアが処理
        var article = await _updateArticleService.ExecuteAsync(id, request);
        return Ok(article);
    }
}
```

## ベストプラクティス

### 1. 適切な例外を選択する

* リソースが見つからない → `NotFoundException`
* 入力値が不正 → `ValidationException`
* 権限不足 → `ForbiddenException`
* リソース競合 → `ConflictException`
* インフラエラー → `InfrastructureException`

### 2. 意味のあるエラーメッセージを提供する

```csharp
// Good
throw new NotFoundException($"Article with ID {id} not found");

// Bad
throw new NotFoundException("Not found");
```

### 3. 追加情報を活用する

```csharp
throw new ValidationException(
    "Invalid article data",
    new Dictionary<string, object>
    {
        ["titleLength"] = title.Length,
        ["maxLength"] = 200
    }
);
```

### 4. 例外の連鎖を保持する

```csharp
catch (DbUpdateException ex)
{
    // innerExceptionを渡すことで、元の例外情報を保持
    throw new InfrastructureException("Database error", ex);
}
```

### 5. try-catchは必要最小限に

* Controllerでは基本的に不要（ミドルウェアが処理）
* Repositoryではインフラ例外の変換時のみ使用
* Serviceでは基本的に不要（必要に応じてthrowのみ）

## まとめ

このエラーハンドリング戦略により：

* ✅ すべての例外を統一的に処理（投げっぱなしなし）
* ✅ try-catchの記述を最小限に抑制（グローバルハンドラーで一括処理）
* ✅ Sentryなどの監視ツールとの連携が容易
* ✅ 統一されたエラーレスポンス形式
* ✅ 適切なHTTPステータスコードの自動設定
* ✅ 各層の責務が明確

AI（GitHub Copilot等）は本ドキュメントを参照することで、適切なエラーハンドリングを実装に反映できます。
