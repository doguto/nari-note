# エラーハンドリング実装例

このドキュメントは、nari-note-backendでのエラーハンドリングの実装例を示します。

## 実装済みファイル

### 1. カスタム例外クラス
- `Src/Application/Exception/ApplicationException.cs` - 基底例外クラス
- `Src/Application/Exception/NotFoundException.cs` - 404エラー
- `Src/Application/Exception/ValidationException.cs` - 400エラー
- `Src/Application/Exception/ConflictException.cs` - 409エラー
- `Src/Application/Exception/UnauthorizedException.cs` - 401エラー
- `Src/Application/Exception/ForbiddenException.cs` - 403エラー
- `Src/Application/Exception/InfrastructureException.cs` - 500エラー

### 2. エラーレスポンスモデル
- `Src/Application/Dto/Response/ErrorResponse.cs`

### 3. グローバル例外ハンドラー
- `Src/Middleware/GlobalExceptionHandlerMiddleware.cs`

### 4. 実装例
- `Src/Application/Service/GetArticleService.cs` - Serviceでの例外スロー例
- `Src/Infrastructure/Repository/ArticleRepository.cs` - Repositoryでのエラーハンドリング例
- `Src/Controller/ArticlesController.cs` - Controllerでの使用例

## 使い方

### Serviceでの例外スロー

```csharp
public class UpdateArticleService
{
    private readonly IArticleRepository _repository;

    public async Task<Article> ExecuteAsync(int id, UpdateArticleRequest request)
    {
        // リソースが見つからない場合
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

        // ビジネスロジック...
        article.Title = request.Title;
        await _repository.UpdateAsync(article);
        
        return article;
    }
}
```

### Repositoryでのエラーハンドリング

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
        // DB制約違反を適切な例外に変換
        if (pgEx.SqlState == "23505") // Unique violation
        {
            throw new ConflictException("Article already exists", ex);
        }
        throw new InfrastructureException("Database error", ex);
    }
}
```

### Controllerでの使用

```csharp
[HttpGet("{id}")]
public async Task<ActionResult> GetArticle(int id)
{
    // try-catchは不要！グローバルミドルウェアが処理
    var article = await _getArticleService.ExecuteAsync(id);
    return Ok(article);
}
```

## エラーレスポンス例

### NotFoundExceptionの場合

リクエスト:
```
GET /api/articles/999
```

レスポンス (404 Not Found):
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Article with ID 999 not found",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles/999",
    "additionalData": null
  }
}
```

### ValidationExceptionの場合

レスポンス (400 Bad Request):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid article data",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": {
      "titleLength": 250,
      "maxLength": 200
    }
  }
}
```

### ConflictExceptionの場合

レスポンス (409 Conflict):
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Article with this title already exists",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": null
  }
}
```

### InfrastructureExceptionの場合

レスポンス (500 Internal Server Error):
```json
{
  "error": {
    "code": "INFRASTRUCTURE_ERROR",
    "message": "Database error occurred while creating article",
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/articles",
    "additionalData": null
  }
}
```

## テスト方法

### 存在しない記事を取得（NotFoundExceptionのテスト）

```bash
curl -X GET http://localhost:5243/api/articles/999
```

期待されるレスポンス: 404 Not Found with error details

### 記事を作成（正常系）

```bash
curl -X POST http://localhost:5243/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Article",
    "body": "This is a test article",
    "authorId": 1,
    "isPublished": true
  }'
```

期待されるレスポンス: 201 Created

## まとめ

- ✅ 全ての例外を統一的に処理
- ✅ Controllerでtry-catchを書く必要なし
- ✅ Serviceでは適切な例外をthrowするだけ
- ✅ Repositoryではインフラエラーを変換
- ✅ エラーレスポンスは統一フォーマット
- ✅ Sentry等の監視ツールと連携可能
