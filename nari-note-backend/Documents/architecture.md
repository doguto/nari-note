# アーキテクチャ

## 基本アーキテクチャの選定

### 前提条件の整理

アーキテクチャは不安定要素の変更に耐える設計になっているべきである。

よってバックエンドアーキテクチャを選定するにあたり、プロジェクトの安定要素と不安定要素を整理する。

**安定要素**
* 基本的技術スタック
  * Front: Next.js
  * Back: ASP.NET Core Web API
  * DB: PostgreSQL
* 開発メンバー
  * 基本的に個人開発
  * 実装スタイル等がぶれる心配は基本的に無し
* アプリ規模
  * 小規模
* モノリシック構成
  * マイクロサービスにはしない

**不安定要素**
* インフラ構成
  * アプリケーションサーバー
  * DBサーバー
  * キャッシュサーバー
* デプロイ方式
  * そもそもインフラ（デプロイ先）が未定
  * 方式も未定
* ユーザー数・データ量
  * そんなに多くは無いと思うが、不確定

### アーキテクチャの候補

**シンプルな階層型アーキテクチャ**

以下の階層にレイヤー分けした運用
* プレゼンテーション層（Controller）
* ビジネスロジック層（Service）
* データアクセス層（Model/Repository）

**クリーンアーキテクチャ**

インフラ構成の未確定をInfrastructure層で吸収できるようにする
以下のレイヤーに分割して運用
* Presentation
  * Controller
* Application
  * UseCase
* Domain
  * Entity
  * ValueObject
  * Repository(Interface)
* Infrastructure
  * Repository(実装)
  * External

### 結論
シンプルな階層型アーキテクチャではインフラ構成の不確定を吸収しきれないが、完全なクリーンアーキテクチャは個人開発には過剰である。
よって、**シンプルな階層型アーキテクチャにData層のみ依存性逆転を適用**する。

**設計方針:**
- Service層はビジネスロジックに専念（キャッシュ等のインフラ関心事は書かない）
- インフラ関心事（キャッシュ、レプリカ振り分け等）はRepository層で吸収
- **API一個につきService一個の粒度**で実装

## アーキテクチャ詳細

### レイヤー構成

```
Controllers
    ↓ 依存
Application (Services, IRepository定義)
    ↓ 依存
Domain (Entities)
    ↑ 実装
Data (DbContext, Repository実装)
```

### プロジェクト構成

```
nari-note-backend/
├── Controller/               # Presentation層
│   ├── ArticlesController.cs
│   ├── UsersController.cs
│   └── TagsController.cs
├── Application/              # Business Logic層
│   ├── Service/
│   │   ├── GetArticleContentService.cs
│   │   ├── CreateArticleService.cs
│   │   ├── UpdateArticleService.cs
│   │   ├── DeleteArticleService.cs
│   │   ├── ToggleLikeService.cs
│   │   └── GetUserProfileService.cs
│   └── Repository/           # Repository抽象化
│       ├── IArticleRepository.cs
│       └── IUserRepository.cs
├── Domain/                   # Domain Entity
│   ├── Article.cs
│   ├── User.cs
│   ├── Tag.cs
│   └── Like.cs
└── Infrastructure/           # Data Access層
    ├── ApplicationDbContext.cs
    ├── ArticleRepository.cs
    └── UserRepository.cs
```

### Entity Framework Core との統合

#### 1. Domain (Entities)
EF Coreのエンティティクラスとして機能。ドメインロジックも含む。

```csharp
// Domain/Article.cs
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties (EF Core用)
    public User Author { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    
    // ドメインロジック
    public bool IsLikedBy(int userId) => Likes.Any(l => l.UserId == userId);
    public int LikeCount => Likes.Count;
}
```

#### 2. Data/ApplicationDbContext
EF Coreのコンテキスト。テーブル定義と設定。

```csharp
// Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<Article> Articles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Like> Likes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Article設定
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            
            // リレーション設定
            entity.HasOne(e => e.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(e => e.AuthorId);
            
            entity.HasMany(e => e.Tags)
                .WithMany(t => t.Articles);
        });
        
        // その他のエンティティ設定...
    }
}
```

#### 3. Application/Repositories (Repository抽象化)
インフラ（DB）からビジネスロジックを分離。

```csharp
// Application/Repositories/IArticleRepository.cs
public interface IArticleRepository
{
    Task<Article?> FindByIdAsync(int id);
    Task<List<Article>> FindByTagAsync(string tagName);
    Task<List<Article>> FindByAuthorAsync(int authorId);
    Task<Article> CreateAsync(Article article);
    Task UpdateAsync(Article article);
    Task DeleteAsync(int id);
}
```

#### 4. Data/Repositories (Repository実装)
EF Coreを使ってRepositoryインターフェースを実装。キャッシュ等のインフラ関心事もここで処理。

```csharp
// Data/Repositories/ArticleRepository.cs
public class ArticleRepository : IArticleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache; // キャッシュもここで扱う
    
    public ArticleRepository(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }
    
    public async Task<Article?> FindByIdAsync(int id)
    {
        // キャッシュチェック
        var cacheKey = $"article:{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<Article>(cached);
        
        // DB取得
        var article = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .Include(a => a.Likes)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        // キャッシュ保存
        if (article != null)
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(article));
        
        return article;
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }
    
    public async Task UpdateAsync(Article article)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
        
        // キャッシュ削除
        await _cache.RemoveAsync($"article:{article.Id}");
    }
}
```

#### 5. Application/Services (ビジネスロジック)
**API一個につきService一個**の粒度でビジネスロジックを実装。

```csharp
// Application/Services/GetArticleContentService.cs
public class GetArticleContentService
{
    private readonly IArticleRepository _articleRepository;
    
    public GetArticleContentService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }
    
    public async Task<Article?> ExecuteAsync(int id)
    {
        return await _articleRepository.FindByIdAsync(id);
    }
}
```

#### 6. Controllers
Serviceを呼び出してHTTPリクエストを処理。

```csharp
// Controllers/ArticlesController.cs
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly GetArticleContentService _getArticleContentService;
    private readonly CreateArticleService _createArticleService;
    private readonly ToggleLikeService _toggleLikeService;
    
    public ArticlesController(
        GetArticleContentService getArticleContentService,
        CreateArticleService createArticleService,
        ToggleLikeService toggleLikeService)
    {
        _getArticleContentService = getArticleContentService;
        _createArticleService = createArticleService;
        _toggleLikeService = toggleLikeService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticle(int id)
    {
        var article = await _getArticleContentService.ExecuteAsync(id);
        if (article == null) return NotFound();
        return Ok(article);
    }
}
```

### DI設定 (Program.cs)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// DbContext登録
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// キャッシュ登録
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Repository登録
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Service登録（API一個につき一個）
builder.Services.AddScoped<GetArticleContentService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### このアーキテクチャのメリット

1. **関心の分離**
   - Service層: ビジネスロジックのみ
   - Repository層: データ取得とインフラ最適化（キャッシュ等）

2. **Service粒度の明確化**
   - API一個につきService一個で責務が明確
   - 各Serviceが単一責任を持つ

3. **テスタビリティ**
   - IRepositoryをモック化してServiceを単体テスト可能
   - キャッシュやDBの実装を気にせずビジネスロジックをテスト

4. **インフラ変更耐性**
   - キャッシュ追加/削除がRepository実装のみで完結
   - DB変更時もRepository実装のみ修正

