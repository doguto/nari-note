# 開発ワークフロー

このドキュメントは、nari-note-backendの開発における一般的なタスクの手順を説明します。

## 目次

1. [環境セットアップ](#環境セットアップ)
2. [新規APIエンドポイントの追加](#新規apiエンドポイントの追加)
3. [データベーススキーマの変更](#データベーススキーマの変更)
4. [便利なコマンド](#便利なコマンド)

---

## 環境セットアップ

### 初回セットアップ

#### Docker Compose使用（推奨）

```bash
# プロジェクトルートから
docker-compose up

# バックグラウンドで起動
docker-compose up -d

# ログ確認
docker-compose logs -f backend
```

#### ローカル環境

```bash
# .NET SDKインストール確認
dotnet --version  # 9.0以上

# PostgreSQLインストールと起動
# macOS
brew install postgresql@16
brew services start postgresql@16

# Ubuntu/Debian
sudo apt install postgresql-16
sudo systemctl start postgresql

# データベース作成
createdb nari_note

# プロジェクトディレクトリに移動
cd nari-note-backend

# 依存関係の復元
dotnet restore

# マイグレーション実行
dotnet ef database update

# アプリケーション起動
dotnet run
```

### 開発サーバーの起動

```bash
# 通常起動
dotnet run

# ホットリロード有効
dotnet watch run
```

アクセス: `http://localhost:5243`

---

## 新規APIエンドポイントの追加

### 例: 記事更新API（PUT /api/articles/{id}）の追加

#### ステップ1: Request/Response DTOの作成

```csharp
using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Dto.Request;

public class UpdateArticleRequest
{
    [Required(ErrorMessage = "タイトルは必須です")]
    [MaxLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "本文は必須です")]
    public string Body { get; set; } = string.Empty;
    
    public bool IsPublished { get; set; }
}
```

```csharp
namespace NariNoteBackend.Application.Dto.Response;

public class UpdateArticleResponse
{
    public int Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### ステップ2: Repository Interfaceにメソッド追加

```csharp
public interface IArticleRepository
{
    // 既存のメソッド...
    
    /// <summary>
    /// 記事を更新する
    /// </summary>
    Task<Article> UpdateAsync(Article article);
}
```

#### ステップ3: Repository実装

```csharp
public async Task<Article> UpdateAsync(Article article)
{
    article.UpdatedAt = DateTime.UtcNow;
    context.Articles.Update(article);
    await context.SaveChangesAsync();
    return article;
}
```

#### ステップ4: Serviceの実装

```csharp
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;
    
    public UpdateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task<UpdateArticleResponse> ExecuteAsync(
        int id,
        UpdateArticleRequest request,
        int userId)
    {
        // 記事の存在確認
        var article = await this.articleRepository.GetByIdAsync(id);
        
        // 権限確認
        if (article.AuthorId != userId)
        {
            throw new ForbiddenException(
                "You don't have permission to update this article");
        }
        
        // 更新
        article.Title = request.Title;
        article.Body = request.Body;
        article.IsPublished = request.IsPublished;
        
        var updated = await this.articleRepository.UpdateAsync(article);
        
        return new UpdateArticleResponse
        {
            Id = updated.Id,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
```

#### ステップ5: Controllerにエンドポイント追加

```csharp
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    readonly UpdateArticleService updateArticleService;
    
    public ArticlesController(
        // 既存のサービス...
        UpdateArticleService updateArticleService)
    {
        // 既存のコンストラクタ処理...
        this.updateArticleService = updateArticleService;
    }
    
    // 既存のエンドポイント...
    
    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateArticleResponse>> UpdateArticle(
        int id,
        [FromBody] UpdateArticleRequest request,
        [FromQuery] int userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var response = await this.updateArticleService.ExecuteAsync(
            id, request, userId);
        return Ok(response);
    }
}
```

#### ステップ6: DI登録

```csharp
// Register services
builder.Services.AddScoped<UpdateArticleService>();
```

#### ステップ7: 動作確認

```bash
# アプリケーション再起動（ホットリロード使用時は不要）
dotnet run

# curlでテスト
curl -X PUT http://localhost:5243/api/articles/1?userId=1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Title",
    "body": "Updated content",
    "isPublished": true
  }'
```

---

## データベーススキーマの変更

### 新しいエンティティの追加

#### ステップ1: Domainエンティティの作成

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NariNoteBackend.Domain;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Article")]
    public int ArticleId { get; set; }

    [Required]
    [ForeignKey("Author")]
    public int AuthorId { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Comment()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Navigation Properties
    public required Article Article { get; set; }
    public required User Author { get; set; }
}
```

#### ステップ2: DbContextに追加

```csharp
public class NariNoteDbContext : DbContext
{
    // 既存のDbSet...
    public DbSet<Comment> Comments { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 既存の設定...
        
        // Commentの設定
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Body).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Article)
                .WithMany()
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId);
        });
    }
}
```

#### ステップ3: マイグレーション作成

```bash
cd nari-note-backend

# マイグレーション作成
dotnet ef migrations add AddCommentEntity

# マイグレーションの確認
cat Migrations/*_AddCommentEntity.cs

# マイグレーション適用
dotnet ef database update
```

#### ステップ4: マイグレーションのロールバック（必要な場合）

```bash
# 一つ前のマイグレーションに戻る
dotnet ef database update <前のマイグレーション名>

# マイグレーションファイルの削除
dotnet ef migrations remove
```

### 既存エンティティの変更

#### 例: Articleにview_countカラムを追加

```csharp
public class Article
{
    // 既存のプロパティ...
    
    public int ViewCount { get; set; } = 0;  // 追加
}
```

```bash
# マイグレーション作成
dotnet ef migrations add AddViewCountToArticle

# 適用
dotnet ef database update
```

---

## 便利なコマンド

### プロジェクト管理

```bash
# パッケージ追加
dotnet add package <パッケージ名>

# パッケージ一覧
dotnet list package

# プロジェクトのクリーン
dotnet clean

# ビルド
dotnet build

# リリースビルド
dotnet build -c Release
```

### Entity Framework Core

```bash
# マイグレーション作成
dotnet ef migrations add <名前>

# マイグレーション適用
dotnet ef database update

# マイグレーション削除（最新のみ）
dotnet ef migrations remove

# データベース削除
dotnet ef database drop

# スクリプト生成（SQLファイル）
dotnet ef migrations script -o migrations.sql
```

### Docker

```bash
# コンテナ起動
docker-compose up -d

# コンテナ停止
docker-compose down

# ログ確認
docker-compose logs -f backend

# コンテナに入る
docker-compose exec backend bash

# PostgreSQLコンテナに接続
docker-compose exec postgres psql -U postgres -d nari_note
```

---

## まとめ

このワークフローに従うことで：
- ✅ 一貫した開発プロセス
- ✅ エラーの早期発見
- ✅ スムーズなデバッグ
- ✅ 効率的な開発

新規機能を追加する際は、このドキュメントと[実装ガイド](./backend-implementation-guide.md)を参照してください。
