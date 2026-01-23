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

## セットアップ

### Docker Composeを使用する場合（推奨）

プロジェクトルートディレクトリから以下のコマンドを実行してください：

```bash
docker-compose up
```

バックエンドAPIサーバーは `http://localhost:5243` で起動します。

### ローカル環境で直接実行する場合

1. PostgreSQLをインストールし、起動してください

2. 依存パッケージをインストール：

```bash
cd nari-note-backend
dotnet restore
```

3. 開発サーバーを起動：

```bash
dotnet run
```

または、ホットリロードを有効にして起動：

```bash
dotnet watch run
```

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
│   │   │   ├── GetArticleService.cs
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

**詳細:** [認証戦略ドキュメント](./docs/authentication-strategy.md#apiエンドポイント)

### 記事（Articles）

- `POST /api/articles` - 記事を作成（認証が必要）
- `GET /api/articles/{id}` - 記事を取得（認証が必要）
- `GET /api/articles/author/{authorId}` - 著者の記事一覧を取得（認証が必要）
- `DELETE /api/articles/{id}` - 記事を削除（認証が必要）

### ユーザー（Users）

- `GET /api/users/{id}` - ユーザープロフィールを取得（認証が必要）

## 開発

### コーディング規約

**詳細は [実装ガイド](./docs/backend-implementation-guide.md) を参照してください**

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
- [アーキテクチャ設計](./docs/architecture.md)
- [実装ガイド](./docs/backend-implementation-guide.md)
- [エラーハンドリング戦略](./docs/error-handling-strategy.md)

### 新規機能の実装手順

1. **Domain Entity の定義** - `Src/Domain/`
2. **マイグレーション作成** - `dotnet ef migrations add`
3. **Repository Interface** - `Src/Application/Repository/`
4. **Repository 実装** - `Src/Infrastructure/Repository/`
5. **Request/Response DTO** - `Src/Application/Dto/`
6. **Service実装** - `Src/Application/Service/`（API一個につきService一個）
7. **Controller実装** - `Src/Controller/`
8. **DI登録** - `Program.cs`

### パッケージの追加

```bash
dotnet add package <パッケージ名>
```

### マイグレーション（Entity Framework Core）

```bash
# マイグレーションの作成
dotnet ef migrations add <マイグレーション名>

# マイグレーションの適用
dotnet ef database update
```

### ビルド

```bash
dotnet build
```

### テスト実行

```bash
dotnet test
```

## 環境変数

| 変数名 | 説明 | デフォルト値 |
|--------|------|------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL接続文字列 | `Host=localhost;Port=5432;Database=nari_note;Username=postgres;Password=postgres` |
| `Jwt:Secret` | JWT署名用の秘密鍵（本番環境では環境変数で設定） | 設定ファイル参照 |
| `Jwt:Issuer` | JWTトークンの発行者 | `nari-note-api` |
| `Jwt:Audience` | JWTトークンの対象者 | `nari-note-client` |
| `Jwt:ExpirationInHours` | JWTトークンの有効期限（時間） | `24` |

### Docker Compose使用時
`docker-compose.yml`で以下の環境変数が自動設定されます：
- `DATABASE_URL`: PostgreSQLコンテナへの接続URL

## 主要な技術詳細

### エラーハンドリング

- **グローバル例外ハンドラーミドルウェア**で統一的に処理
- カスタム例外クラス（`NotFoundException`, `ValidationException`, `ConflictException` 等）
- Controllerでtry-catchは不要
- 統一されたエラーレスポンス形式

**詳細:** [エラーハンドリング戦略](./docs/error-handling-strategy.md)

### データベース設計

- PostgreSQL 16 + Entity Framework Core 9.0
- マイグレーションベースのスキーマ管理
- リレーションシップ定義（Article-User, Article-Tag, Like, Follow等）

**詳細:** [ER図](./docs/er-diagram.md)

## 関連ドキュメント

- [実装ガイド](./docs/backend-implementation-guide.md) - コーディング規約と実装パターン
- [アーキテクチャ](./docs/architecture.md) - システムアーキテクチャの設計思想
- [エラーハンドリング戦略](./docs/error-handling-strategy.md) - エラーハンドリングの包括的ガイド
- [認証戦略](./docs/authentication-strategy.md) - 認証システムの設計と実装（API使用方法を含む）
- [ER図](./docs/er-diagram.md) - データベース設計
- [開発ワークフロー](./docs/development-workflow.md) - 開発手順とタスクガイド

## その他

- ポート: 5243
- HTTPS リダイレクトが有効化されています
- ヘルスチェックエンドポイントが `/health` で利用可能です
