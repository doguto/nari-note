# アーキテクチャ概要（実装用）

このドキュメントは、nari-note-backendの実装時に参照するアーキテクチャの概要です。
設計思想や選定過程については [architecture.md](./architecture.md) を参照してください。

## レイヤー構成

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

## ディレクトリ構成

```
nari-note-backend/
├── Src/
│   ├── Controller/            # プレゼンテーション層（APIエンドポイント）
│   │   ├── ApplicationController.cs  # 基底Controller（認証用）
│   │   ├── ArticlesController.cs
│   │   └── UsersController.cs
│   ├── Application/           # アプリケーション層
│   │   ├── Service/          # ビジネスロジック（API一個につきService一個）
│   │   │   ├── CreateArticleService.cs
│   │   │   ├── GetArticleContentService.cs
│   │   │   └── UpdateArticleService.cs
│   │   ├── Dto/              # Data Transfer Objects
│   │   │   ├── Request/      # リクエストDTO
│   │   │   └── Response/     # レスポンスDTO
│   │   ├── Exception/        # カスタム例外クラス
│   │   │   └── NariNoteException.cs
│   │   └── Security/         # セキュリティ関連（JWT等）
│   ├── Domain/                # ドメイン層
│   │   ├── Repository/       # Repository抽象化（インターフェース）
│   │   │   ├── IRepository.cs  # 共通基底インターフェース
│   │   │   ├── IArticleRepository.cs
│   │   │   └── IUserRepository.cs
│   │   └── Entity/           # エンティティ
│   │       ├── EntityBase.cs  # 共通基底クラス
│   │       ├── Article.cs
│   │       └── User.cs
│   ├── Infrastructure/        # インフラストラクチャ層
│   │   ├── NariNoteDbContext.cs  # EF Core DbContext
│   │   └── Repository/       # Repository実装
│   │       ├── ArticleRepository.cs
│   │       └── UserRepository.cs
│   ├── Middleware/            # カスタムミドルウェア
│   │   ├── GlobalExceptionHandlerMiddleware.cs
│   │   └── AuthenticationMiddleware.cs
│   ├── Filter/                # アクションフィルタ
│   │   └── ValidateModelStateAttribute.cs
│   └── Extension/             # 拡張メソッド
└── Program.cs                 # アプリケーションのエントリーポイント
```

## 各レイヤーの責務

### Controller層
- HTTPリクエスト/レスポンスの処理
- 入力バリデーション（ValidateModelStateAttribute）
- 適切なHTTPステータスコードの返却
- Serviceの呼び出し（API一個につきService一個）

### Service層
- ビジネスロジックの実装
- 複数のRepositoryの協調
- ドメインオブジェクトの操作
- 認可チェック（必要な場合）

### Repository層（Interface）
- データアクセスの抽象化
- ドメイン層への依存を定義

### Repository層（Implementation）
- EF Coreを使ったデータアクセス実装
- エンティティのInclude（Eager Loading）
- インフラ関心事（将来: キャッシュ、レプリカ振り分け等）

### Domain層
- データ構造の定義（Entity）
- ドメインロジック
- EF Core属性の付与

## 重要な設計原則

1. **API一個につきService一個**
   - `GET /api/articles/{id}` → `GetArticleContentService`
   - `POST /api/articles` → `CreateArticleService`
   - 各Serviceが単一責任を持つ

2. **依存性逆転（Repository層のみ）**
   - DomainがRepository Interfaceを定義
   - InfrastructureがRepository Interfaceを実装
   - Serviceはインフラの詳細を知らない

3. **エラーハンドリングの統一**
   - 全レイヤーでtry-catchは基本的に不要
   - 適切な例外をthrow
   - GlobalExceptionHandlerMiddlewareが統一的に処理

4. **認証の統一**
   - 認証が必要なControllerは `ApplicationController` を継承
   - `UserId` プロパティで認証済みユーザーIDにアクセス

## データフロー例

### 記事作成API（POST /api/articles）

1. **Controller** - `ArticlesController.CreateArticle`
   - リクエストを受け取る
   - ValidateModelStateでバリデーション
   - CreateArticleServiceを呼び出す

2. **Service** - `CreateArticleService.ExecuteAsync`
   - リクエストDTOからArticle Entityを作成
   - ArticleRepositoryを呼び出す
   - レスポンスDTOを作成して返す

3. **Repository** - `ArticleRepository.CreateAsync`
   - EF CoreでArticleをDBに保存
   - 保存されたArticleを返す

4. **Controller** - HTTPレスポンスを返す
   - CreatedAtActionで201レスポンス

## 関連ドキュメント

- **[architecture-overview.md](./architecture-overview.md)** - 設計思想と選定過程
- **[backend-implementation-guide.md](./backend-implementation-guide.md)** - 実装パターンとコーディング規約
- **[error-handling-strategy.md](./error-handling-strategy.md)** - エラーハンドリングの詳細
