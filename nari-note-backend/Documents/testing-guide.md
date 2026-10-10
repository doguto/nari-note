# テストガイド

バックエンドのテストの方針と書き方をまとめます。

## 実行方法

```bash
cd nari-note-backend

# 全テスト（結合テストは Docker が必要）
dotnet test

# 単体テストのみ（Docker 不要）
dotnet test --filter "FullyQualifiedName~NariNoteBackend.Tests.Application"

# 結合テストのみ
dotnet test --filter "FullyQualifiedName~NariNoteBackend.Tests.Controller"
```

## 構成

| 種別 | 対象 | 依存の扱い | 配置 |
|---|---|---|---|
| 単体テスト | Service | Repository / Gateway の interface を NSubstitute で差し替え | `Tests/NariNoteBackend.Tests/Application/Service/` |
| 結合テスト | Controller（HTTP 経由） | 実 Service・実 Repository・実 PostgreSQL（Testcontainers） | `Tests/NariNoteBackend.Tests/Controller/` |

```
Tests/NariNoteBackend.Tests/
├── Application/Service/     # Service の単体テスト
├── Controller/              # Controller の結合テスト
└── Support/
    ├── Builder/             # テストデータの Builder
    ├── Fake/                # 外部サービス（メール・Discord・画像ストレージ）の Fake
    ├── Integration/         # NariNoteApiFactory / IntegrationTestBase
    └── TestTimeProvider.cs  # 時刻を固定・変更できる TimeProvider
```

使用ライブラリ: xUnit v3 / NSubstitute / Microsoft.AspNetCore.Mvc.Testing / Testcontainers / Respawn

## 共通ルール

- テストクラス名は `{対象クラス名}Test`、テストメソッド名は日本語で「何がどうなるか」を書く
- アサーションは xUnit の `Assert` を使う
- **テストデータは Builder で作る**。既定値で有効な Entity になるので、テストに関係する項目だけ `With*` 等で上書きする
- **現在時刻は `TestTimeProvider` で制御する**。既定値は `TestTimeProvider.DefaultUtcNow`（2026-01-01 00:00:00 UTC）。テスト内で `DateTime.UtcNow` は使わない
- 開発用の `DataSeeder` はテストでは使わない
- いいね・フォロー・タグなど設定項目の少ない Entity は `TestEntity` で作る（`TestEntity.Like(user, article)` 等）

```csharp
var author = new UserBuilder().Build();
var draft = new ArticleBuilder(author).WithTitle("下書き").Draft().Build();
var scheduled = new ArticleBuilder(author).PublishedAt(TestTimeProvider.DefaultUtcNow.AddHours(1)).Build();
```

### どちらのテストを書くか

新しい API を追加したら、Service の単体テストと Controller の結合テストの両方を書く。

- **単体テスト**: すべての Service に `{Service名}Test` を用意する。分岐や境界値（有効期限・公開日時など）を網羅し、Repository に渡す引数とレスポンスへの詰め替えを検証する
- **結合テスト**: すべてのエンドポイントについて、正常系・認証・権限・バリデーション・公開状態による出し分けを検証する。クエリの正しさ（絞り込み・並び順）は単体テストでは検証できないため、こちらで確認する

### テストで不具合を見つけた場合

本来あるべき挙動でテストを書き、Issue を起票したうえで、修正するまでは理由と Issue 番号を付けて `Skip` する。現状の誤った挙動を期待値にしない。

```csharp
[Fact(Skip = "既知の不具合 (#570): KifuRepository.ReplaceAllByArticleIdAsync が余分な既存の棋譜を削除しない")]
public async Task 棋譜を減らして更新すると余分な棋譜は削除される()
```

不具合を修正する際は `Skip` を外して、テストが通ることを確認する。

## 単体テスト（Service）

Service はコンストラクタで受け取る interface を `Substitute.For<T>()` で差し替えて直接生成します。

```csharp
public class CreateArticleServiceTest
{
    readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    readonly TestTimeProvider timeProvider = new();
    ...

    [Fact]
    public async Task 公開指定で公開日時が未指定なら現在時刻で公開される()
    {
        var request = CreateRequest();
        request.IsPublished = true;

        await this.service.ExecuteAsync(request);

        await this.articleRepository.Received(1).CreateAsync(
            Arg.Is<Article>(a => a.PublishedAt == TestTimeProvider.DefaultUtcNow)
        );
    }
}
```

### 注意: Vogen の ID 型に `Arg.Any` は使えない

`Arg.Any<ArticleId>()` は内部で `default(ArticleId)` を返しますが、Vogen の未初期化値は等価比較が成立しないため、NSubstitute が引数を特定できず例外になります。「任意の ID」を指定したい場合は `ForAnyArgs` / `WithAnyArgs` を使います。

```csharp
// ❌ 例外になる
this.commentRepository.FindByArticleAsync(Arg.Any<ArticleId>()).Returns(comments);

// ✅ 任意の引数にマッチさせる（渡す値は何でもよい）
this.commentRepository
    .FindByArticleAsync(ArticleId.From(Guid.CreateVersion7()))
    .ReturnsForAnyArgs(comments);

await this.kifuRepository.DidNotReceiveWithAnyArgs().ReplaceAllByArticleIdAsync(
    ArticleId.From(Guid.CreateVersion7()),
    null!
);
```

特定の ID を指定する場合（`FindForceByIdAsync(article.Id)` 等）や、`Arg.Is<Article>(...)` のような Entity への使用は問題ありません。

### HttpResponse を受け取る Service

`SignInService` のように `HttpResponse` へ Cookie を書き込む Service は、`new DefaultHttpContext().Response` を渡し、`Headers.SetCookie` を検証する。

```csharp
readonly HttpResponse httpResponse = new DefaultHttpContext().Response;
...
Assert.Contains("authToken=jwt-token", this.httpResponse.Headers.SetCookie.ToString());
```

### Builder でナビゲーションプロパティを設定する

単体テストでは Repository が読み込み済みの Entity を返す想定になるため、タグ・いいね・講座の記事は Builder で設定する。

```csharp
var article = new ArticleBuilder(author).WithTags("振り飛車").LikedBy(liker).Build();
var course = new CourseBuilder(owner).WithArticles(article).LikedBy(liker).Build();
```
| `Factory.EmailHelper` 等 | 外部サービスの Fake（送信内容の検証） |

## 結合テスト（Controller）

`IntegrationTestBase` を継承します。API は `NariNoteApiFactory` が 1 回だけ起動し、結合テスト全体で共有します。

- **テストごとに DB は空になる**（Respawn）。必要なデータは各テストの Arrange で `SeedAsync` する
- 時計と Fake の記録もテストごとに初期化される
- 結合テスト同士は直列に実行される

```csharp
public class ArticlesControllerTest : IntegrationTestBase
{
    public ArticlesControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task 下書きは作者本人だけが取得できる()
    {
        var author = new UserBuilder().Build();
        var draft = new ArticleBuilder(author).Draft().Build();
        await SeedAsync(author, draft);
        var url = $"/api/articles/{draft.Id.Value}";

        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient().GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClientAs(author).GetAsync(url)).StatusCode);
    }
}
```

| ヘルパー | 用途 |
|---|---|
| `CreateClient()` | 未認証のクライアント |
| `CreateClientAs(user)` | 指定ユーザーで認証済みのクライアント（`user` は事前に `SeedAsync` しておく） |
| `SeedAsync(...)` | Entity を DB に投入する |
| `QueryAsync(db => ...)` | DB の状態を検証する |
| `ReadAsync<T>(response)` | API と同じ JSON 設定でレスポンスを読む |
| `CreateClientWithToken(token)` | 任意の認証トークンを Cookie に持つクライアント |
| `GetAuthToken(response)` | レスポンスの `Set-Cookie` から認証トークンを取り出す |
| `ProcessOutboxAsync()` | Outbox に溜まったメッセージを配送する（メール送信等の検証用） |
| `TimeProvider` | 時刻の変更（`SetUtcNow` / `Advance`） |

### 本番との違い

- 環境名は `Testing`（SSM・シードデータは読み込まない）
- メール・Discord・画像ストレージは Fake に差し替え
- `OutboxWorker`（バックグラウンド処理）は起動しない。Outbox の配送まで検証する場合は `ProcessOutboxAsync()` を呼ぶ
- 認証 Cookie は `Secure` 属性付きで発行されるため、`HttpClient` が自動では送り返さない。サインイン後の状態を検証する場合は `GetAuthToken` と `CreateClientWithToken` を使う
- リクエストボディは API の契約を検証するため、DTO ではなく匿名オブジェクトで組み立てる
