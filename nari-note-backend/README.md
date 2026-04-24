# nari-note-backend

nari-noteプロジェクトのバックエンドAPIサーバーです。

## 技術スタック

- **フレームワーク**: ASP.NET Core 9.0
- **言語**: C# (.NET 9.0)
- **データベース**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0
- **アーキテクチャ**: レイヤードアーキテクチャ（Controller、Application、Domain、Infrastructure）

## 必要な環境

- .NET 9.0 SDK
- Docker & Docker Compose（推奨）
- PostgreSQL 16（ローカル実行の場合）

## プロジェクト構成

```
nari-note-backend/
├── Src/
│   ├── Controller/            # プレゼンテーション層（APIエンドポイント）
│   │   ├── ArticlesController.cs
│   │   ├── UsersController.cs
│   │   └── HealthController.cs
│   ├── Application/           # アプリケーション層
│   │   ├── Service/          # ビジネスロジック（API一個につきService一個）
│   │   │   ├── CreateArticleService.cs
│   │   │   ├── GetArticleContentService.cs
│   │   │   └── DeleteArticleService.cs
│   │   ├── Repository/       # Repository抽象化（インターフェース）
│   │   │   ├── IArticleRepository.cs
│   │   │   └── IUserRepository.cs
│   │   ├── Dto/              # Data Transfer Objects
│   │   │   ├── Request/      # リクエストDTO
│   │   │   └── Response/     # レスポンスDTO
│   │   └── Exception/        # カスタム例外クラス
│   │       ├── ApplicationException.cs（基底クラス）
│   │       ├── NotFoundException.cs
│   │       ├── ValidationException.cs
│   │       ├── ConflictException.cs
│   │       └── InfrastructureException.cs
│   ├── Domain/                # ドメイン層
│   │   ├── Article.cs        # エンティティ（EF Coreエンティティ + ドメインロジック）
│   │   ├── User.cs
│   │   ├── Tag.cs
│   │   └── Like.cs
│   ├── Infrastructure/        # インフラストラクチャ層
│   │   ├── NariNoteDbContext.cs  # EF Core DbContext
│   │   └── Repository/       # Repository実装
│   │       ├── ArticleRepository.cs
│   │       └── UserRepository.cs
│   └── Middleware/            # カスタムミドルウェア
│       └── GlobalExceptionHandlerMiddleware.cs
├── Migrations/                # Entity Framework Coreマイグレーション
├── Program.cs                 # アプリケーションのエントリーポイント
├── appsettings.json          # アプリケーション設定
├── Dockerfile                # Docker設定
└── nari-note-backend.csproj  # プロジェクトファイル
```

## API エンドポイント

### ヘルスチェック

- `GET /health` - アプリケーションの健全性チェック
- `GET /api/Health` - APIの健全性チェック

### 認証（Auth）

- `POST /api/auth/signup` - ユーザー登録
- `POST /api/auth/signin` - ログイン

**詳細:** [認証戦略ドキュメント](./Documents/authentication-strategy.md#apiエンドポイント)

### 記事（Articles）

- `POST /api/articles` - 記事を作成（認証が必要）
- `GET /api/articles/{id}` - 記事を取得（認証が必要）
- `GET /api/articles/author/{authorId}` - 著者の記事一覧を取得（認証が必要）
- `DELETE /api/articles/{id}` - 記事を削除（認証が必要）

### ユーザー（Users）

- `GET /api/users/{id}` - ユーザープロフィールを取得（認証が必要）

## 開発

### コーディング規約

**詳細は [実装ガイド](./Documents/backend-implementation-guide.md) を参照してください**

主要な規約：
- private変数: アンダースコア無し、キャメルケース、`this.`でアクセス
- private修飾子: クラスフィールドでは省略
- 日付時刻: 常に `DateTime.UtcNow` を使用

### アーキテクチャ

レイヤードアーキテクチャ + Repository パターン

```
Controller → Service → Repository Interface
                            ↓
                       Domain Entity
                            ↑
                  Repository Implementation
```

**詳細は以下のドキュメントを参照:**
- [アーキテクチャ設計](./Documents/architecture.md)
- [実装ガイド](./Documents/backend-implementation-guide.md)
- [エラーハンドリング戦略](./Documents/error-handling-strategy.md)

### 新規機能の実装手順

1. **Domain Entity の定義** - `Src/Domain/`
2. **マイグレーション作成** - `dotnet ef migrations add`
3. **Repository Interface** - `Src/Application/Repository/`
4. **Repository 実装** - `Src/Infrastructure/Repository/`
5. **Request/Response DTO** - `Src/Application/Dto/`
6. **Service実装** - `Src/Application/Service/`（API一個につきService一個）
7. **Controller実装** - `Src/Controller/`
8. **DI登録** - `Program.cs`

## 主要な技術詳細

### エラーハンドリング

- **グローバル例外ハンドラーミドルウェア**で統一的に処理
- カスタム例外クラス（`NotFoundException`, `ValidationException`, `ConflictException` 等）
- Controllerでtry-catchは不要
- 統一されたエラーレスポンス形式

**詳細:** [エラーハンドリング戦略](./Documents/error-handling-strategy.md)

### データベース設計

- PostgreSQL 16 + Entity Framework Core 9.0
- マイグレーションベースのスキーマ管理
- リレーションシップ定義（Article-User, Article-Tag, Like, Follow等）

**詳細:** [ER図](./Documents/er-diagram.md)

## 関連ドキュメント

- [実装ガイド](./Documents/backend-implementation-guide.md) - コーディング規約と実装パターン
- [アーキテクチャ](./Documents/architecture.md) - システムアーキテクチャの設計思想
- [エラーハンドリング戦略](./Documents/error-handling-strategy.md) - エラーハンドリングの包括的ガイド
- [認証戦略](./Documents/authentication-strategy.md) - 認証システムの設計と実装（API使用方法を含む）
- [ER図](./Documents/er-diagram.md) - データベース設計
- [開発ワークフロー](./Documents/development-workflow.md) - 開発手順とタスクガイド

## その他

- ポート: 5243
- HTTPS リダイレクトが有効化されています
- ヘルスチェックエンドポイントが `/health` で利用可能です
