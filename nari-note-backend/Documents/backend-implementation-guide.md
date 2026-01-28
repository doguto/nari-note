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
Domain (Repository Interface)
    ↓
Domain (Entity)
    ↑
Infrastructure (Repository実装)
```

### ディレクトリ構成

> **注意**: 現在 Repository Interface は `Application/Repository/` に配置されていますが、将来的に `Domain/Repository/` に移動する予定です。新規実装時は `Domain/Repository/` に配置してください。

```
nari-note-backend/
├── Src/
│   ├── Controller/            # プレゼンテーション層（APIエンドポイント）
│   │   └── ApplicationController.cs  # 基底Controller
│   ├── Application/           # アプリケーション層
│   │   ├── Service/          # ビジネスロジック（API一個につきService一個）
│   │   ├── Dto/              # Data Transfer Objects
│   │   │   ├── Request/      # リクエストDTO
│   │   │   └── Response/     # レスポンスDTO
│   │   ├── Exception/        # カスタム例外クラス
│   │   └── Security/         # セキュリティ関連（JWT等）
│   ├── Domain/                # ドメイン層
│   │   ├── Repository/       # Repository抽象化（インターフェース）※新規実装はここに配置
│   │   │   └── IRepository.cs  # 共通Repository基底インターフェース
│   │   └── Entity/           # エンティティ
│   │       └── EntityBase.cs  # 共通Entity基底クラス
│   ├── Infrastructure/        # インフラストラクチャ層
│   │   ├── NariNoteDbContext.cs  # EF Core DbContext
│   │   └── Repository/       # Repository実装
│   ├── Middleware/            # カスタムミドルウェア
│   │   ├── GlobalExceptionHandlerMiddleware.cs
│   │   └── AuthenticationMiddleware.cs
│   ├── Filter/                # アクションフィルタ
│   │   └── ValidateModelStateAttribute.cs
│   └── Extension/             # 拡張メソッド
├── Migrations/                # Entity Framework Coreマイグレーション
└── Program.cs                 # アプリケーションのエントリーポイント
```

### Controller層

#### 基本パターン

```csharp
using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ApplicationController
{
    readonly CreateArticleService createArticleService;
    readonly GetArticleContentService getArticleContentService;
    readonly DeleteArticleService deleteArticleService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        GetArticleContentService getArticleContentService,
        DeleteArticleService deleteArticleService)
    {
        this.createArticleService = createArticleService;
        this.getArticleContentService = getArticleContentService;
        this.deleteArticleService = deleteArticleService;
    }
    
    [HttpPost]
    [ValidateModelState]
    public async Task<ActionResult> CreateArticle(
        [FromBody] CreateArticleRequest request)
    {
        var response = await createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(int id)
    {
        var request = new GetArticleRequest { Id = id };
        var response = await getArticleService.ExecuteAsync(request);
        return Ok(response);
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

#### ApplicationController基底クラス

認証が必要なControllerは `ApplicationController` を継承します。

```csharp
using Microsoft.AspNetCore.Mvc;

namespace NariNoteBackend.Controller;

public abstract class ApplicationController : ControllerBase
{
    protected int UserId => (int)HttpContext.Items["UserId"]!;
}
```

- **UserId**: AuthenticationMiddlewareで設定される認証済みユーザーのID
- 認証が必要なエンドポイントで `UserId` を使用してアクセス可能

#### Controllerの責務

- **HTTPリクエスト/レスポンスの処理**
- **入力バリデーション（ValidateModelStateAttribute使用）**
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
   - `[ValidateModelState]` フィルタを使用してバリデーション
   - 手動でModelStateをチェックする必要はない

5. **認証が必要な場合**
   - `ApplicationController` を継承
   - `UserId` プロパティで認証済みユーザーIDにアクセス

#### ValidateModelStateAttribute フィルタ

バリデーションエラーを自動的に処理するアクションフィルタ：

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NariNoteBackend.Filter;

public class ValidateModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            );
            context.Result = new BadRequestObjectResult(new { errors });
        }
    }
}
```

**使用方法：**
```csharp
[HttpPost]
[ValidateModelState]  // このフィルタを付与
public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
{
    // ModelStateは既にチェック済み
    var response = await createArticleService.ExecuteAsync(request);
    return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
}
```

---

### Service層

#### 基本パターン

```csharp
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    readonly IArticleRepository articleRepository;
    
    public CreateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<CreateArticleResponse> ExecuteAsync(CreateArticleRequest request)
    {
        // 1. ドメインオブジェクトの作成
        var article = new Article
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished
            // CreatedAt, UpdatedAt は EntityBase で自動設定
        };
        
        // 2. Repositoryの呼び出し
        var createdArticle = await articleRepository.CreateAsync(article);
        
        // 3. レスポンスDTOの作成
        return new CreateArticleResponse
        {
            Id = createdArticle.Id,
            CreatedAt = createdArticle.CreatedAt
        };
    }
}
```

#### 認証が必要なServiceの例

```csharp
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;
    
    public UpdateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    // UserIdを第一引数として受け取る
    public async Task<UpdateArticleResponse> ExecuteAsync(int userId, UpdateArticleRequest request)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        
        // 認可チェック
        if (article.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("この記事を編集する権限がありません");
        }
        
        // 更新処理
        if (request.Title != null) article.Title = request.Title;
        if (request.Body != null) article.Body = request.Body;
        if (request.IsPublished != null) article.IsPublished = request.IsPublished.Value;
        article.UpdatedAt = DateTime.UtcNow;
        
        await articleRepository.UpdateWithTagAsync(article, request.Tags);
        
        return new UpdateArticleResponse { Id = article.Id };
    }
}
```

#### Serviceの責務

- **ビジネスロジックの実装**
- **複数のRepositoryの協調**
- **ドメインオブジェクトの操作**
- **適切な例外のスロー**
- **認可チェック（必要な場合）**

#### Service実装のルール

1. **API一個につきService一個の粒度**
   - `GET /api/articles/{id}` → `GetArticleContentService`
   - `POST /api/articles` → `CreateArticleService`
   - `DELETE /api/articles/{id}` → `DeleteArticleService`

2. **メソッド名は `ExecuteAsync`**
   - 統一されたインターフェース
   - 認証が必要な場合: `ExecuteAsync(int userId, TRequest request)`
   - 認証が不要な場合: `ExecuteAsync(TRequest request)` または `ExecuteAsync(int id)`

3. **Request/Response DTOを使用**
   - 入力は基本 `Request` 必要に応じてパラメータ（id等）を追加
   - 出力は `Response`

4. **try-catchは不要**
   - 必要に応じて適切な例外をthrow
   - グローバルミドルウェアがキャッチ

5. **インフラ関心事は書かない**
   - キャッシュ、トランザクション等はRepository層

6. **EntityBase継承による自動フィールド設定**
   - `CreatedAt`, `UpdatedAt` はEntityBaseで自動設定
   - 明示的に設定する必要がある場合のみ上書き

---

### Repository層

#### 共通Repository基底インターフェース

すべてのRepositoryは `IRepository<T>` を継承します：

```csharp
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Repository;

public interface IRepository<T> where T : EntityBase
{
    Task<T> CreateAsync(T entity);
    Task<T?> FindByIdAsync(int id);
    Task<T> FindForceByIdAsync(int id);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

- **FindByIdAsync**: 見つからない場合は `null` を返す
- **FindForceByIdAsync**: 見つからない場合は `KeyNotFoundException` をthrow

#### 個別Repository Interface定義

```csharp
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Application.Repository;

public interface IArticleRepository : IRepository<Article>
{
    Task<List<Article>> FindByAuthorAsync(int authorId);
    
    Task<List<Article>> FindByTagAsync(string tagName);
    
    Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null);
}
```

#### Repository実装

```csharp
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain.Entity;

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
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article> FindForceByIdAsync(int id)
    {
        var article = await FindByIdAsync(id);
        if (article == null)
        {
            throw new KeyNotFoundException($"記事{id}が存在しません");
        }
        return article;
    }

    public async Task<Article> UpdateAsync(Article entity)
    {
        context.Articles.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<Article>> FindByAuthorAsync(int authorId)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .Where(a => a.AuthorId == authorId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<Article>> FindByTagAsync(string tagName)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Likes)
            .Where(a => a.ArticleTags.Any(at => EF.Functions.ILike(at.Tag.Name, tagName)))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Article> UpdateWithTagAsync(Article article, List<string>? tagNames = null)
    {
        context.Articles.Update(article);
        
        // Update tags if provided
        if (tagNames != null)
        {
            // Remove existing ArticleTags
            var existingArticleTags = await context.ArticleTags
                .Where(at => at.ArticleId == article.Id)
                .ToListAsync();
            context.ArticleTags.RemoveRange(existingArticleTags);
            
            if (tagNames.Count > 0)
            {
                // Get existing tags
                var existingTags = await context.Tags
                    .Where(t => tagNames.Contains(t.Name))
                    .ToListAsync();
                
                var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();
                var newTagNames = tagNames.Where(tn => !existingTagNames.Contains(tn)).ToList();
                
                // Create new tags
                var newTags = newTagNames.Select(name => new Tag
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
                
                if (newTags.Count > 0)
                {
                    context.Tags.AddRange(newTags);
                    await context.SaveChangesAsync(); // Save to get Tag IDs
                }
                
                // Combine all tags
                var allTags = existingTags.Concat(newTags).ToList();
                
                // Create ArticleTag associations
                var articleTags = allTags.Select(tag => new ArticleTag
                {
                    ArticleId = article.Id,
                    TagId = tag.Id,
                    CreatedAt = DateTime.UtcNow,
                    Article = article,
                    Tag = tag
                }).ToList();
                
                context.ArticleTags.AddRange(articleTags);
            }
        }
        
        await context.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(int id)
    {
        var article = await context.Articles.FindAsync(id);
        if (article != null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }
    }
}
```

#### Repositoryの責務

- **データアクセス（CRUD操作）**
- **EF Coreの操作**
- **エンティティの取得時に必要なIncludeの指定**
- **インフラ関心事（将来: キャッシュ、レプリカ振り分け等）**

#### Repository実装のルール

1. **メソッド命名規則**
   - `Find*Async`: 見つからない場合は `null` を返す
   - `FindForce*Async`: 見つからない場合は `KeyNotFoundException` をthrow
   - `Create*Async`: 作成して返す
   - `Update*Async`: 更新して返す
   - `Delete*Async`: 削除（返り値なし）

2. **Include（Eager Loading）**
   - 必要なナビゲーションプロパティは明示的にInclude
   ```csharp
   .Include(a => a.Author)
   .Include(a => a.ArticleTags)
       .ThenInclude(at => at.Tag)
   .Include(a => a.Likes)
   ```

3. **例外処理**
   - 基本的に**try-catchは使用しない**
   - DB制約違反等はEF Coreが投げる例外をそのまま伝播
   - GlobalExceptionHandlerMiddlewareが適切なレスポンスに変換
   - 特定の例外ハンドリングが必要な場合のみtry-catchを使用

4. **KeyNotFoundExceptionの使用**
   - `FindForceByIdAsync` では、存在しない場合に `KeyNotFoundException` をthrow
   - この例外はGlobalExceptionHandlerMiddlewareで404 Not Foundに変換される

---

### Domain層

#### EntityBase基底クラス

すべてのエンティティは `EntityBase` を継承します：

```csharp
namespace NariNoteBackend.Domain.Entity;

public abstract class EntityBase
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- `CreatedAt`, `UpdatedAt` はデフォルトで自動設定される
- 必要に応じて上書き可能

#### Entityパターン

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain.Entity;

public class Article : EntityBase
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

    // Navigation Properties
    public User Author { get; set; }
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

1. **EntityBase継承**
   - すべてのエンティティは `EntityBase` を継承
   - `CreatedAt`, `UpdatedAt` は自動的に設定される
   - 明示的なコンストラクタで初期化する必要はない

2. **Data Annotations使用**
   - `[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`

3. **required修飾子**
   - null許容でないプロパティには `required` を付与

4. **ナビゲーションプロパティ**
   - 関連エンティティは初期化（`= new()`）または required
   - EF Coreが外部キーから自動で設定するため、requiredは必須ではない

5. **ドメインロジック**
   - シンプルな計算プロパティやメソッドを実装可能

6. **namespaceは `NariNoteBackend.Domain.Entity`**
   - Entityは `Domain/Entity/` ディレクトリ配下に配置

#### ValueObject（Vogen）

エンティティのIDには**Vogen**ライブラリを使用した型安全なValueObjectを使用します：

```csharp
using Vogen;

namespace NariNoteBackend.Domain.ValueObject;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct ArticleId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct UserId;
```

**ValueObjectの利点:**
- IDの型が明確になり、誤った型の混在を防止
- 型安全性により、コンパイル時にエラーを検出
- EF Coreとの統合が容易

**使用例:**
```csharp
public class Article : EntityBase
{
    [Key]
    public ArticleId Id { get; set; }
    
    [Required]
    public UserId AuthorId { get; set; }
}
```

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

## まとめ

このガイドに従うことで、以下のメリットが得られます：

- ✅ 一貫したコーディングスタイル
- ✅ レイヤー間の責務の明確化
- ✅ エラーハンドリングの統一
- ✅ 保守性の高いコード
- ✅ AIによる自動実装の精度向上

### 新規機能実装チェックリスト

新規機能を実装する際は、以下のチェックリストを参考にしてください：

#### 1. 設計フェーズ
- [ ] 機能要件を確認
- [ ] 必要なEntityを特定
- [ ] API仕様を確認（エンドポイント、リクエスト/レスポンス形式）

#### 2. 実装フェーズ

**Entity作成:**
- [ ] `Src/Domain/Entity/` にEntityクラスを作成
- [ ] `EntityBase` を継承
- [ ] Data Annotations（`[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`）を付与
- [ ] ナビゲーションプロパティを定義
- [ ] 必要に応じてドメインロジックを実装

**Repository Interface:**
- [ ] `Src/Domain/Repository/` にインターフェースを作成
- [ ] `IRepository<TEntity>` を継承
- [ ] エンティティ固有のメソッドを定義（例: `FindByAuthorAsync`）

**Repository実装:**
- [ ] `Src/Infrastructure/Repository/` に実装クラスを作成
- [ ] インターフェースを実装
- [ ] 必要な `Include` を追加（Eager Loading）
- [ ] `FindForceByIdAsync` では `KeyNotFoundException` をthrow
- [ ] try-catchは基本的に使用しない

**Request/Response DTO:**
- [ ] `Src/Application/Dto/Request/` にRequestクラスを作成
- [ ] バリデーション属性を付与（`[Required]`, `[MaxLength]`, `[Range]` 等）
- [ ] エラーメッセージは日本語で記述
- [ ] `Src/Application/Dto/Response/` にResponseクラスを作成（必要に応じて）

**Service実装:**
- [ ] `Src/Application/Service/` にServiceクラスを作成
- [ ] API一個につきService一個の粒度
- [ ] 必要なRepositoryをコンストラクタで注入
- [ ] `ExecuteAsync` メソッドを実装
  - [ ] 認証が必要な場合: `ExecuteAsync(int userId, TRequest request)`
  - [ ] 認証が不要な場合: `ExecuteAsync(TRequest request)` または `ExecuteAsync(int id)`
- [ ] ビジネスロジックを実装
- [ ] 適切な例外をthrow（try-catchは不要）
- [ ] EntityBaseによる自動フィールド設定を活用

**Controller実装:**
- [ ] `Src/Controller/` にControllerクラスを作成
- [ ] 認証が必要な場合は `ApplicationController` を継承
- [ ] 認証が不要な場合は `ControllerBase` を継承
- [ ] `[ApiController]`, `[Route("api/[controller]")]` を付与
- [ ] 必要なServiceをコンストラクタで注入
- [ ] HTTPメソッド属性を付与（`[HttpGet]`, `[HttpPost]` 等）
- [ ] バリデーションが必要な場合は `[ValidateModelState]` を付与
- [ ] 適切なHTTPステータスコードを返す
- [ ] try-catchは不要

**DI登録:**
- [ ] ServiceとRepositoryを `ApplicationServiceInstaller.cs` または `InfrastructureServiceInstaller.cs` に登録
- [ ] 既存の登録パターンに従う

#### 3. 検証フェーズ
- [ ] コンパイルエラーがないことを確認（`dotnet build`）
- [ ] 手動でAPIをテスト（`dotnet run` または Docker）
- [ ] 正常系の動作確認
- [ ] 異常系の動作確認（バリデーションエラー、404等）
- [ ] エラーレスポンスの形式が正しいことを確認

#### 4. コードレビュー
- [ ] コーディング規約に従っているか確認
- [ ] private変数の命名（アンダースコア無し）
- [ ] `DateTime.UtcNow` を使用しているか
- [ ] try-catchを不要な箇所で使用していないか
- [ ] レイヤー間の責務が守られているか
- [ ] 既存のパターンと一貫性があるか

### 参考ドキュメント

実装時は以下のドキュメントを参照してください：

1. **このガイド（backend-implementation-guide.md）** - 基本的な実装パターン
2. **[エラーハンドリング戦略](./error-handling-strategy.md)** - 例外処理の詳細
3. **[アーキテクチャ](./architecture.md)** - 設計思想とレイヤー構成
4. **既存のコード** - 同様の機能を参考にする

AI（GitHub Copilot等）は本ドキュメントを参照することで、適切な実装を自動生成できます。
