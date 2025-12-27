# バックエンド実装ガイド

このドキュメントは、nari-note-backendの実装パターンと規約を定義します。
AI（GitHub Copilot等）が一貫した実装を行えるように、具体的なコード例とともに説明します。

## 目次

1. [コーディング規約](#コーディング規約)
2. [レイヤー別実装パターン](#レイヤー別実装パターン)
3. [DTO設計パターン](#dto設計パターン)
4. [開発ワークフロー](#開発ワークフロー)

---

## コーディング規約

### 命名規則

#### private変数
- **アンダースコア（`_`）のプレフィックスを付けない**
- **キャメルケースで命名**
- **readonly修飾子を使用**

```csharp
// ✅ 正しい例
public class ArticleService
{
    readonly IArticleRepository articleRepository;
    readonly ILogger<ArticleService> logger;
    
    public ArticleService(
        IArticleRepository articleRepository,
        ILogger<ArticleService> logger)
    {
        this.articleRepository = articleRepository;
        this.logger = logger;
    }
}

// ❌ 間違った例
public class ArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly ILogger<ArticleService> _logger;
}
```

### アクセス修飾子

#### privateの省略
- **クラスのフィールドにおいて、`private`修飾子は省略**

```csharp
// ✅ 正しい例
public class ArticleService
{
    readonly IArticleRepository articleRepository;
}

// ❌ 間違った例
public class ArticleService
{
    private readonly IArticleRepository articleRepository;
}
```

#### その他のアクセス修飾子
- `public`, `protected`, `internal` は明示的に記述

```csharp
public class Article
{
    public int Id { get; set; }
    protected virtual void OnCreated() { }
}
```

### 日付時刻の扱い

- **常に `DateTime.UtcNow` を使用**
- ローカル時刻（`DateTime.Now`）は使用しない

```csharp
// ✅ 正しい例
var article = new Article
{
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// ❌ 間違った例
var article = new Article
{
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now
};
```

---

## レイヤー別実装パターン

### アーキテクチャ概要

```
Controller (プレゼンテーション層)
    ↓
Application (Service層)
    ↓
Application (Repository Interface)
    ↓
Domain (Entity)
    ↑
Infrastructure (Repository実装)
```

### Controller層

#### 基本パターン

```csharp
using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    readonly CreateArticleService createArticleService;
    readonly GetArticleService getArticleService;
    readonly DeleteArticleService deleteArticleService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        GetArticleService getArticleService,
        DeleteArticleService deleteArticleService)
    {
        this.createArticleService = createArticleService;
        this.getArticleService = getArticleService;
        this.deleteArticleService = deleteArticleService;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateArticleResponse>> CreateArticle(
        [FromBody] CreateArticleRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            );
            return BadRequest(new { errors });
        }
        
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var response = await createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Article>> GetArticle(int id)
    {
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var article = await getArticleService.ExecuteAsync(id);
        return Ok(article);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id, [FromQuery] int userId)
    {
        var request = new DeleteArticleRequest { Id = id, UserId = userId };
        await deleteArticleService.ExecuteAsync(request);
        return NoContent();
    }
}
```

#### Controllerの責務

- **HTTPリクエスト/レスポンスの処理**
- **入力バリデーション（ModelState）**
- **適切なHTTPステータスコードの返却**
- **Serviceの呼び出し**

#### Controller実装のルール

1. **try-catchは不要**
   - グローバル例外ハンドラーミドルウェアが処理
   
2. **API一個につきService一個を呼び出す**
   - 複数のServiceを組み合わせない
   
3. **ビジネスロジックは書かない**
   - Serviceに委譲する

4. **ModelStateのバリデーション**
   - リクエストDTOのバリデーション属性をチェック

---

### Service層

#### 基本パターン

```csharp
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    readonly IArticleRepository articleRepository;
    readonly IUserRepository userRepository;
    
    public CreateArticleService(
        IArticleRepository articleRepository,
        IUserRepository userRepository)
    {
        this.articleRepository = articleRepository;
        this.userRepository = userRepository;
    }
    
    public async Task<CreateArticleResponse> ExecuteAsync(CreateArticleRequest request)
    {
        // 1. ビジネスロジックの検証
        var author = await userRepository.FindByIdAsync(request.AuthorId);
        if (author == null)
        {
            throw new NotFoundException($"Author with ID {request.AuthorId} not found");
        }
        
        // 2. ドメインオブジェクトの作成
        var article = new Article
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null! // EF Core handles navigation property via AuthorId
        };
        
        // 3. Repositoryの呼び出し
        var created = await articleRepository.CreateAsync(article);
        
        // 4. レスポンスDTOの作成
        return new CreateArticleResponse
        {
            Id = created.Id,
            CreatedAt = created.CreatedAt
        };
    }
}
```

#### Serviceの責務

- **ビジネスロジックの実装**
- **複数のRepositoryの協調**
- **ドメインオブジェクトの操作**
- **適切な例外のスロー**

#### Service実装のルール

1. **API一個につきService一個の粒度**
   - `GET /api/articles/{id}` → `GetArticleService`
   - `POST /api/articles` → `CreateArticleService`
   - `DELETE /api/articles/{id}` → `DeleteArticleService`

2. **メソッド名は `ExecuteAsync`**
   - 統一されたインターフェース

3. **Request/Response DTOを使用**
   - 入力は `Request`, 出力は `Response` または `Entity`

4. **try-catchは不要**
   - 必要に応じて適切な例外をthrow
   - グローバルミドルウェアがキャッチ

5. **インフラ関心事は書かない**
   - キャッシュ、トランザクション等はRepository層

---

### Repository層

#### Interface定義

```csharp
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository
{
    /// <summary>
    /// 記事を作成する
    /// </summary>
    Task<Article> CreateAsync(Article article);
    
    /// <summary>
    /// IDで記事を検索（存在しない場合はnullを返す）
    /// </summary>
    Task<Article?> FindByIdAsync(int id);
    
    /// <summary>
    /// IDで記事を取得（存在しない場合はNotFoundExceptionをthrow）
    /// </summary>
    Task<Article> GetByIdAsync(int id);
    
    /// <summary>
    /// 著者IDで記事一覧を取得
    /// </summary>
    Task<List<Article>> FindByAuthorAsync(int authorId);
    
    /// <summary>
    /// 記事を削除する
    /// </summary>
    Task DeleteAsync(int id);
}
```

#### Repository実装

```csharp
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class ArticleRepository : IArticleRepository
{
    readonly NariNoteDbContext context;
    
    public ArticleRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        try
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
            return article;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            // DB制約違反を適切な例外に変換
            if (pgEx.SqlState == "23505") // Unique constraint violation
            {
                throw new ConflictException("Article with this title already exists", ex);
            }
            if (pgEx.SqlState == "23503") // Foreign key violation
            {
                throw new ValidationException("Invalid reference to related entity", null, ex);
            }
            // その他のDB例外
            throw new InfrastructureException(
                "Database error occurred while creating article", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The article was modified by another user", ex);
        }
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await context.Articles
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Article> GetByIdAsync(int id)
    {
        var article = await FindByIdAsync(id);
        
        if (article == null)
        {
            throw new NotFoundException($"Article with ID {id} not found");
        }
        
        return article;
    }
    
    public async Task<List<Article>> FindByAuthorAsync(int authorId)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Where(a => a.AuthorId == authorId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var article = await context.Articles.FindAsync(id);
            if (article != null)
            {
                context.Articles.Remove(article);
                await context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException(
                "The article was modified or deleted by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InfrastructureException(
                $"Database error occurred while deleting article with ID {id}", ex);
        }
    }
}
```

#### Repositoryの責務

- **データアクセス（CRUD操作）**
- **EF Coreの操作**
- **DB例外の変換**
- **インフラ関心事（将来: キャッシュ、レプリカ振り分け等）**

#### Repository実装のルール

1. **メソッド命名規則**
   - `Find*Async`: 見つからない場合は `null` を返す
   - `Get*Async`: 見つからない場合は `NotFoundException` をthrow
   - `Create*Async`: 作成して返す
   - `Update*Async`: 更新（返り値なし or Entity）
   - `Delete*Async`: 削除（返り値なし）

2. **Include（Eager Loading）**
   - 必要なナビゲーションプロパティは明示的にInclude
   ```csharp
   .Include(a => a.Author)
   .Include(a => a.ArticleTags)
       .ThenInclude(at => at.Tag)
   ```

3. **DB例外の変換**
   - `DbUpdateException` → 適切な `ApplicationException` に変換
   - PostgreSQL エラーコードで判定
     - `23505`: Unique制約違反 → `ConflictException`
     - `23503`: 外部キー制約違反 → `ValidationException`

4. **try-catchの使用**
   - Repositoryではtry-catchを使用してDB例外を変換

---

### Domain層

#### Entityパターン

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain;

public class Article
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Author")]
    public int AuthorId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public required string Body { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Article()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Navigation Properties
    public required User Author { get; set; }
    public List<ArticleTag> ArticleTags { get; set; } = new();
    public List<Like> Likes { get; set; } = new();

    // Domain Logic
    public bool IsLikedBy(int userId) => Likes.Any(l => l.UserId == userId);
    public int LikeCount => Likes.Count;
}
```

#### Entityの責務

- **データ構造の定義**
- **EF Core属性の付与**
- **ナビゲーションプロパティの定義**
- **シンプルなドメインロジック**

#### Entity実装のルール

1. **Data Annotations使用**
   - `[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`

2. **required修飾子**
   - null許容でないプロパティには `required` を付与

3. **ナビゲーションプロパティ**
   - 関連エンティティは `required` または初期化

4. **ドメインロジック**
   - シンプルな計算プロパティやメソッドを実装可能

---

## DTO設計パターン

### Request DTO

```csharp
using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

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
    
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; } = false;
}
```

### Response DTO

```csharp
namespace NariNoteBackend.Application.Dto.Response;

public class CreateArticleResponse
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### DTO設計のルール

1. **Request DTOにはバリデーション属性を付与**
   - `[Required]`, `[MaxLength]`, `[Range]` 等
   - 日本語のエラーメッセージ

2. **Response DTOは必要最小限のプロパティ**
   - クライアントが必要とする情報のみ

3. **EntityをそのままResponse DTOとして使用可能**
   - シンプルなGETの場合は変換不要

---

## 開発ワークフロー

### 1. 新規機能の追加

#### ステップ1: Domainエンティティの定義
```bash
# 必要に応じてエンティティを追加・修正
vim nari-note-backend/Src/Domain/Article.cs
```

#### ステップ2: マイグレーションの作成
```bash
cd nari-note-backend
dotnet ef migrations add <マイグレーション名>
dotnet ef database update
```

#### ステップ3: Repository Interfaceの定義
```bash
# Application/Repository に Interface を作成
vim nari-note-backend/Src/Application/Repository/IArticleRepository.cs
```

#### ステップ4: Repository実装
```bash
# Infrastructure/Repository に実装を作成
vim nari-note-backend/Src/Infrastructure/Repository/ArticleRepository.cs
```

#### ステップ5: Request/Response DTOの作成
```bash
# Application/Dto/Request, Response を作成
vim nari-note-backend/Src/Application/Dto/Request/CreateArticleRequest.cs
vim nari-note-backend/Src/Application/Dto/Response/CreateArticleResponse.cs
```

#### ステップ6: Serviceの実装
```bash
# Application/Service にServiceを作成
vim nari-note-backend/Src/Application/Service/CreateArticleService.cs
```

#### ステップ7: Controllerの実装
```bash
# Controller にControllerを作成
vim nari-note-backend/Src/Controller/ArticlesController.cs
```

#### ステップ8: Program.csにDI登録
```csharp
// Repository登録
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();

// Service登録
builder.Services.AddScoped<CreateArticleService>();
```

### 2. ビルドとテスト

```bash
# ビルド
dotnet build

# 実行
dotnet run

# または、ホットリロード
dotnet watch run
```

### 3. Docker環境での実行

```bash
# プロジェクトルートから
docker-compose up
```

---

## まとめ

このガイドに従うことで、以下のメリットが得られます：

- ✅ 一貫したコーディングスタイル
- ✅ レイヤー間の責務の明確化
- ✅ エラーハンドリングの統一
- ✅ 保守性の高いコード
- ✅ AIによる自動実装の精度向上

新規機能を実装する際は：
1. このガイドのパターンに従う
2. `error-handling-strategy.md` でエラーハンドリングを確認
3. 既存のコードを参考にする

AI（GitHub Copilot等）は本ドキュメントを参照することで、適切な実装を自動生成できます。
