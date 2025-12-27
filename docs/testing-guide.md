# テスト実装ガイド

このドキュメントは、nari-note-backendでのテスト実装の方針とパターンを定義します。

## 目次

1. [テスト戦略](#テスト戦略)
2. [テスト環境のセットアップ](#テスト環境のセットアップ)
3. [ユニットテスト](#ユニットテスト)
4. [統合テスト](#統合テスト)
5. [テストデータ管理](#テストデータ管理)
6. [テスト実行](#テスト実行)

---

## テスト戦略

### テストピラミッド

```
        /\
       /  \  E2E Tests (少数)
      /____\
     /      \  Integration Tests (中程度)
    /________\
   /          \  Unit Tests (多数)
  /____________\
```

### テストの方針

1. **ユニットテストを中心に**
   - Service層とRepository層を重点的にテスト
   - 各クラスを独立してテスト
   - モックを活用して依存関係を排除

2. **統合テストで全体動作を確認**
   - Controller→Service→Repository→DBの一連の流れをテスト
   - 実際のデータベースを使用（TestContainersまたはインメモリDB）

3. **E2Eテストは最小限**
   - フロントエンドとの統合はフロントエンド側で実施
   - バックエンド単体のE2Eは統合テストでカバー

---

## テスト環境のセットアップ

### テストプロジェクトの作成

```bash
cd nari-note-backend

# xUnitテストプロジェクト作成
dotnet new xunit -n nari-note-backend.Tests

# テストプロジェクトをソリューションに追加（ソリューションがある場合）
dotnet sln add nari-note-backend.Tests/nari-note-backend.Tests.csproj

# メインプロジェクトへの参照を追加
cd nari-note-backend.Tests
dotnet add reference ../nari-note-backend.csproj
```

### 必要なパッケージのインストール

```bash
cd nari-note-backend.Tests

# テストフレームワーク（xUnit）は既に含まれている

# モックライブラリ
dotnet add package Moq

# アサーションライブラリ（より読みやすいアサーション）
dotnet add package FluentAssertions

# EF Core InMemory（統合テスト用）
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# WebApplicationFactory（統合テスト用）
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

### プロジェクト構成

```
nari-note-backend.Tests/
├── Unit/                          # ユニットテスト
│   ├── Services/
│   │   ├── CreateArticleServiceTests.cs
│   │   ├── GetArticleServiceTests.cs
│   │   └── DeleteArticleServiceTests.cs
│   └── Repositories/
│       ├── ArticleRepositoryTests.cs
│       └── UserRepositoryTests.cs
├── Integration/                   # 統合テスト
│   ├── Controllers/
│   │   ├── ArticlesControllerTests.cs
│   │   └── UsersControllerTests.cs
│   └── TestHelpers/
│       ├── TestDbContextFactory.cs
│       └── CustomWebApplicationFactory.cs
└── Fixtures/                      # テストデータ
    ├── ArticleFixtures.cs
    └── UserFixtures.cs
```

---

## ユニットテスト

### Service層のユニットテスト

#### 基本パターン

```csharp
using Moq;
using Xunit;
using FluentAssertions;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Tests.Unit.Services;

public class CreateArticleServiceTests
{
    readonly Mock<IArticleRepository> mockArticleRepository;
    readonly CreateArticleService service;

    public CreateArticleServiceTests()
    {
        this.mockArticleRepository = new Mock<IArticleRepository>();
        this.service = new CreateArticleService(this.mockArticleRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsCreatedArticle()
    {
        // Arrange
        var request = new CreateArticleRequest
        {
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true
        };

        var expectedArticle = new Article
        {
            Id = 1,
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null!
        };

        this.mockArticleRepository
            .Setup(r => r.CreateAsync(It.IsAny<Article>()))
            .ReturnsAsync(expectedArticle);

        // Act
        var result = await this.service.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        this.mockArticleRepository.Verify(
            r => r.CreateAsync(It.Is<Article>(a =>
                a.Title == request.Title &&
                a.Body == request.Body &&
                a.AuthorId == request.AuthorId &&
                a.IsPublished == request.IsPublished)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var request = new CreateArticleRequest
        {
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true
        };

        this.mockArticleRepository
            .Setup(r => r.CreateAsync(It.IsAny<Article>()))
            .ThrowsAsync(new ConflictException("Article already exists"));

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => this.service.ExecuteAsync(request));
    }
}
```

#### GetArticleServiceのテスト例

```csharp
public class GetArticleServiceTests
{
    readonly Mock<IArticleRepository> mockArticleRepository;
    readonly GetArticleService service;

    public GetArticleServiceTests()
    {
        this.mockArticleRepository = new Mock<IArticleRepository>();
        this.service = new GetArticleService(this.mockArticleRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ArticleExists_ReturnsArticle()
    {
        // Arrange
        var expectedArticle = new Article
        {
            Id = 1,
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        this.mockArticleRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(expectedArticle);

        // Act
        var result = await this.service.ExecuteAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Article");
        result.Author.Should().NotBeNull();
        result.Author.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task ExecuteAsync_ArticleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        this.mockArticleRepository
            .Setup(r => r.GetByIdAsync(999))
            .ThrowsAsync(new NotFoundException("Article with ID 999 not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.service.ExecuteAsync(999));
        
        exception.Message.Should().Contain("999");
    }
}
```

### Repository層のユニットテスト（InMemory DB使用）

```csharp
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Repository;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Tests.Unit.Repositories;

public class ArticleRepositoryTests : IDisposable
{
    readonly NariNoteDbContext context;
    readonly ArticleRepository repository;

    public ArticleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<NariNoteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new NariNoteDbContext(options);
        this.repository = new ArticleRepository(this.context);

        // シードデータ
        SeedTestData();
    }

    void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        this.context.Users.Add(user);
        this.context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_ValidArticle_ReturnsCreatedArticle()
    {
        // Arrange
        var article = new Article
        {
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null!
        };

        // Act
        var result = await this.repository.CreateAsync(article);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Test Article");
        
        var dbArticle = await this.context.Articles.FindAsync(result.Id);
        dbArticle.Should().NotBeNull();
        dbArticle!.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetByIdAsync_ArticleExists_ReturnsArticle()
    {
        // Arrange
        var article = new Article
        {
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null!
        };
        this.context.Articles.Add(article);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByIdAsync(article.Id);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Article");
        result.Author.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ArticleNotFound_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => this.repository.GetByIdAsync(999));
    }

    [Fact]
    public async Task FindByAuthorAsync_AuthorHasArticles_ReturnsArticles()
    {
        // Arrange
        var article1 = new Article
        {
            Title = "Article 1",
            Body = "Body 1",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            Author = null!
        };
        var article2 = new Article
        {
            Title = "Article 2",
            Body = "Body 2",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Author = null!
        };
        this.context.Articles.AddRange(article1, article2);
        await this.context.SaveChangesAsync();

        // Act
        var results = await this.repository.FindByAuthorAsync(1);

        // Assert
        results.Should().HaveCount(2);
        results[0].Title.Should().Be("Article 2"); // 新しい順
        results[1].Title.Should().Be("Article 1");
    }

    public void Dispose()
    {
        this.context.Database.EnsureDeleted();
        this.context.Dispose();
    }
}
```

---

## 統合テスト

### Controller統合テスト

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Tests.Integration.Controllers;

public class ArticlesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    readonly HttpClient client;
    readonly CustomWebApplicationFactory factory;

    public ArticlesControllerTests(CustomWebApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateArticle_ValidRequest_ReturnsCreated()
    {
        // Arrange
        await SeedTestUser();
        
        var request = new CreateArticleRequest
        {
            Title = "Integration Test Article",
            Body = "Integration Test Body",
            AuthorId = 1,
            IsPublished = true
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreateArticleResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetArticle_ArticleExists_ReturnsOk()
    {
        // Arrange
        var articleId = await SeedTestArticle();

        // Act
        var response = await this.client.GetAsync($"/api/articles/{articleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var article = await response.Content.ReadFromJsonAsync<Article>();
        article.Should().NotBeNull();
        article!.Id.Should().Be(articleId);
        article.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetArticle_ArticleNotFound_ReturnsNotFound()
    {
        // Act
        var response = await this.client.GetAsync("/api/articles/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task DeleteArticle_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var articleId = await SeedTestArticle();

        // Act
        var response = await this.client.DeleteAsync($"/api/articles/{articleId}?userId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // 削除確認
        var getResponse = await this.client.GetAsync($"/api/articles/{articleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    async Task SeedTestUser()
    {
        using var scope = this.factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
        
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    async Task<int> SeedTestArticle()
    {
        await SeedTestUser();
        
        using var scope = this.factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
        
        var article = new Article
        {
            Title = "Test Article",
            Body = "Test Body",
            AuthorId = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = null!
        };
        
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        
        return article.Id;
    }
}
```

### CustomWebApplicationFactory

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NariNoteBackend.Infrastructure;

namespace NariNoteBackend.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContextを削除
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<NariNoteDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // InMemory Databaseを使用
            services.AddDbContext<NariNoteDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // データベース初期化
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
            context.Database.EnsureCreated();
        });
    }
}
```

---

## テストデータ管理

### Fixtureパターン

```csharp
namespace NariNoteBackend.Tests.Fixtures;

public static class ArticleFixtures
{
    public static Article CreateValidArticle(
        int id = 1,
        string title = "Test Article",
        string body = "Test Body",
        int authorId = 1,
        bool isPublished = true)
    {
        return new Article
        {
            Id = id,
            Title = title,
            Body = body,
            AuthorId = authorId,
            IsPublished = isPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Author = UserFixtures.CreateValidUser(authorId)
        };
    }

    public static CreateArticleRequest CreateValidRequest(
        string title = "Test Article",
        string body = "Test Body",
        int authorId = 1,
        bool isPublished = true)
    {
        return new CreateArticleRequest
        {
            Title = title,
            Body = body,
            AuthorId = authorId,
            IsPublished = isPublished
        };
    }
}

public static class UserFixtures
{
    public static User CreateValidUser(
        int id = 1,
        string name = "Test User",
        string email = "test@example.com")
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
```

---

## テスト実行

### コマンドライン

```bash
# すべてのテストを実行
dotnet test

# 詳細な出力
dotnet test --verbosity normal

# 特定のテストクラスのみ実行
dotnet test --filter FullyQualifiedName~CreateArticleServiceTests

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD（GitHub Actions）

```yaml
name: Backend Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
      working-directory: ./nari-note-backend
    
    - name: Build
      run: dotnet build --no-restore
      working-directory: ./nari-note-backend
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
      working-directory: ./nari-note-backend
```

---

## ベストプラクティス

### 1. テスト命名規則

```
[メソッド名]_[テストシナリオ]_[期待される結果]
```

例:
- `ExecuteAsync_ValidRequest_ReturnsCreatedArticle`
- `GetByIdAsync_ArticleNotFound_ThrowsNotFoundException`

### 2. AAA（Arrange-Act-Assert）パターン

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange: テストデータと依存関係の準備
    var request = new CreateArticleRequest { ... };
    
    // Act: テスト対象メソッドの実行
    var result = await service.ExecuteAsync(request);
    
    // Assert: 結果の検証
    result.Should().NotBeNull();
}
```

### 3. モックの検証

```csharp
// メソッドが正しく呼ばれたか検証
mockRepository.Verify(
    r => r.CreateAsync(It.IsAny<Article>()),
    Times.Once);

// 特定の引数で呼ばれたか検証
mockRepository.Verify(
    r => r.CreateAsync(It.Is<Article>(a => a.Title == "Test")),
    Times.Once);
```

### 4. テストの独立性

- 各テストは他のテストに依存しない
- データベースを使用する場合は各テストでクリーンアップ
- `IDisposable`パターンでリソース解放

### 5. 例外テスト

```csharp
// xUnit
await Assert.ThrowsAsync<NotFoundException>(
    () => service.ExecuteAsync(999));

// FluentAssertions
await service.Invoking(s => s.ExecuteAsync(999))
    .Should().ThrowAsync<NotFoundException>()
    .WithMessage("*999*");
```

---

## まとめ

このテスト戦略により：
- ✅ 高品質なコードの保証
- ✅ リファクタリングの安全性確保
- ✅ バグの早期発見
- ✅ ドキュメントとしての役割
- ✅ CI/CDパイプラインでの自動検証

新機能を実装する際は、必ずテストも同時に実装してください。
