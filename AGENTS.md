# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## プロジェクト概要

nari-noteは将棋記事の投稿・共有プラットフォームです。フロントエンドとバックエンドが分離されたモダンなWebアプリケーションです。

### 技術スタック

**バックエンド**
- ASP.NET Core 9.0 (.NET 9.0)
- PostgreSQL 16 + Entity Framework Core 9.0
- JWT認証（System.IdentityModel.Tokens.Jwt）
- BCrypt.Net-Next（パスワードハッシュ化）
- Vogen（型安全なValueObject）

**フロントエンド**
- Next.js 15 (App Router)
- React 19
- TypeScript 5.9
- TanStack Query（React Query）v5.90
- Axios
- Tailwind CSS 4
- shadcn/ui（Radix UI）
- Tiptap（リッチテキストエディタ）

## よく使うコマンド

### 開発環境の起動

```bash
# Docker Composeで全サービス起動（推奨）
docker-compose up

# バックグラウンドで起動
docker-compose up -d

# サービス停止
docker-compose down
```

**アクセスURL:**
- フロントエンド: http://localhost:3000
- バックエンドAPI: http://localhost:5243
- PostgreSQL: localhost:5432

### バックエンド（nari-note-backend/）

```bash
# 開発サーバー起動（ホットリロード有効）
dotnet watch run

# 通常起動
dotnet run

# ビルド
dotnet build

# テスト実行
dotnet test

# 依存パッケージの復元
dotnet restore

# 新しいパッケージの追加
dotnet add package <パッケージ名>

# EF Core マイグレーション作成
dotnet ef migrations add <マイグレーション名>

# マイグレーション適用
dotnet ef database update

# マイグレーション削除（最後に追加されたもの）
dotnet ef migrations remove
```

### フロントエンド（nari-note-frontend/）

```bash
# 開発サーバー起動
npm run dev

# プロダクションビルド
npm run build

# プロダクションサーバー起動
npm start

# Linter実行
npm run lint

# 依存パッケージのインストール
npm install

# 新しいパッケージの追加
npm install <パッケージ名>
```

## アーキテクチャ

### バックエンド：レイヤードアーキテクチャ + Repository パターン

```
Controller（プレゼンテーション層）
    ↓ 依存
Application（Service層）
    ↓ 依存
Domain（Repository Interface + Entity）
    ↑ 実装
Infrastructure（Repository実装 + DbContext）
```

**重要な設計原則:**
- **API一個につきService一個**の粒度で実装
- Serviceはビジネスロジックに専念（キャッシュ等のインフラ関心事は書かない）
- インフラ関心事はRepository層で吸収
- グローバル例外ハンドラーミドルウェアで統一的にエラー処理（Controllerでtry-catchは不要）

**ディレクトリ構造:**
```
nari-note-backend/Src/
├── Controller/              # APIエンドポイント
├── Application/
│   ├── Service/            # ビジネスロジック（API一個につきService一個）
│   ├── Dto/
│   │   ├── Request/        # リクエストDTO
│   │   └── Response/       # レスポンスDTO
│   └── Exception/          # カスタム例外（NotFoundException, ValidationException等）
├── Domain/
│   ├── Entity/             # エンティティ（EF Core + ドメインロジック）
│   ├── Repository/         # Repository Interface（新規実装はここに配置）
│   └── Security/           # JWT等のセキュリティ関連
├── Infrastructure/
│   ├── NariNoteDbContext.cs  # EF Core DbContext
│   └── Repository/         # Repository実装
├── Middleware/             # グローバルミドルウェア
└── Filter/                 # アクションフィルタ
```

### フロントエンド：Atomic Designパターン

```
Atoms（最小単位のコンポーネント）
    ↓ 組み合わせ
Molecules（機能コンポーネント）
    ↓ 組み合わせ
Organisms（完全な機能ブロック）
```

**重要な設計原則:**
- コンポーネントは小さな単位（Atoms → Molecules → Organisms）で階層的に構成
- 共通コンポーネント（Atoms/Molecules）は `components/common/` に配置
- 機能固有のOrganismsは `features/{feature}/organisms/` に配置
- データフェッチングが複雑な場合はContainer/Presentationalパターンを併用

**ディレクトリ構造:**
```
nari-note-frontend/src/
├── app/                    # Next.js App Router
│   ├── (auth)/            # 認証関連ページグループ
│   ├── articles/          # 記事関連ページ
│   └── users/             # ユーザー関連ページ
├── components/
│   ├── common/
│   │   ├── atoms/         # 最小単位（FormField, ErrorAlert, TagChip等）
│   │   ├── molecules/     # 機能単位（EmailField, PasswordField, TagInput等）
│   │   ├── Loading.tsx
│   │   ├── ErrorMessage.tsx
│   │   └── EmptyState.tsx
│   ├── ui/                # 基本UIコンポーネント（shadcn/ui）
│   └── layout/            # レイアウトコンポーネント
├── features/              # 機能ごとのモジュール
│   ├── article/
│   │   └── organisms/     # ArticleFormPage, ArticleDetailPage等
│   ├── auth/
│   │   └── organisms/     # LoginPage, SignUpPage等
│   └── user/
│       └── organisms/     # UserProfilePage等
└── lib/
    ├── api/               # APIクライアント（TanStack Query + Axios）
    ├── providers/         # Reactプロバイダー
    ├── utils/             # ユーティリティ関数
    └── hooks/             # 共通カスタムフック
```

## コーディング規約

### バックエンド（C#）

**命名規則:**
- private変数: アンダースコア（`_`）無し、キャメルケース、`readonly`推奨
  - 例: `readonly IArticleRepository articleRepository;`
- private変数へのアクセス: `this.`を使用
  - 例: `this.articleRepository.FindByIdAsync(id);`
- private修飾子: クラスフィールドでは省略（デフォルトでprivate）
  - 良い例: `readonly NariNoteDbContext context;`
  - 悪い例: `private readonly NariNoteDbContext context;`

**ValueObject（ID型）:**
- エンティティのIDにはVogenライブラリを使用した型安全なValueObjectを使用
  - 例: `ArticleId`, `UserId`, `TagId`
  - `NariNoteBackend.Domain.ValueObject` 名前空間に定義

**日付時刻:**
- 常に `DateTime.UtcNow` を使用（`DateTime.Now` は禁止）

**エラーハンドリング:**
- Controllerでtry-catchは不要（グローバル例外ハンドラーが処理）
- カスタム例外を使用: `NotFoundException`, `ValidationException`, `ConflictException`

### フロントエンド（TypeScript）

**Atomic Design実装順序:**
1. Atomsの確認・作成（`components/common/atoms/`）
2. Moleculesの確認・作成（`components/common/molecules/`）
3. Organismsの実装（`features/{feature}/organisms/`）

**型定義:**
- すべてのpropsに明示的な型定義を追加
- API型は `lib/api/types.ts` に定義

**コンポーネント:**
- 共通コンポーネント（Loading, ErrorMessage, EmptyState）を活用
- データフェッチングはTanStack Queryのフックを使用
  - 例: `useGetArticle`, `useCreateArticle`

## 新機能実装の手順

### バックエンド

1. **Domain Entity の定義** (`Src/Domain/Entity/`)
   - EF Coreエンティティ + ドメインロジック
2. **マイグレーション作成**
   ```bash
   dotnet ef migrations add <マイグレーション名>
   dotnet ef database update
   ```
3. **Repository Interface** (`Src/Domain/Repository/`)
4. **Repository 実装** (`Src/Infrastructure/Repository/`)
5. **Request/Response DTO** (`Src/Application/Dto/`)
6. **Service実装** (`Src/Application/Service/`)
   - API一個につきService一個
7. **Controller実装** (`Src/Controller/`)
8. **DI登録** (`Program.cs`)

### フロントエンド

1. **API型定義** (`lib/api/types.ts`)
2. **APIエンドポイント関数** (`lib/api/endpoints.ts`)
3. **TanStack Queryフック** (`lib/api/hooks.ts`)
4. **Atoms作成** (`components/common/atoms/`)
   - 必要な基本要素の確認・作成
5. **Molecules作成** (`components/common/molecules/`)
   - Atomsを組み合わせた機能コンポーネント
6. **Organisms実装** (`features/{feature}/organisms/`)
   - 完全な機能ブロック
7. **ページ実装** (`app/`)
   - Next.js App Routerのページコンポーネント

## 環境変数

### バックエンド

設定ファイル: `nari-note-backend/appsettings.json`, `appsettings.Development.json`

- `ConnectionStrings:DefaultConnection`: PostgreSQL接続文字列
- `Jwt:Secret`: JWT署名用の秘密鍵
- `Jwt:Issuer`: JWTトークンの発行者（デフォルト: `nari-note-api`）
- `Jwt:Audience`: JWTトークンの対象者（デフォルト: `nari-note-client`）
- `Jwt:ExpirationInHours`: 有効期限（デフォルト: 24時間）

### フロントエンド

設定ファイル: `nari-note-frontend/.env.local`

- `NEXT_PUBLIC_API_URL`: バックエンドAPIのURL（デフォルト: `http://localhost:5243`）

## データベース

- **DBMS**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0
- **マイグレーション**: EF Core Migrations（`Migrations/` ディレクトリ）
- **開発環境**: Docker Composeで自動マイグレーション + シードデータ投入

## 認証

- **方式**: JWT（JSON Web Token）
- **パスワードハッシュ**: BCrypt.Net-Next
- **ミドルウェア**: `JwtAuthenticationMiddleware` で認証処理
- **エンドポイント**:
  - `POST /api/auth/signup`: ユーザー登録
  - `POST /api/auth/signin`: ログイン

## 重要なドキュメント

プロジェクトの詳細な実装ガイドは以下を参照してください：

**バックエンド:**
- `nari-note-backend/Documents/backend-implementation-guide.md` - コーディング規約と実装パターン（最重要）
- `nari-note-backend/Documents/architecture.md` - アーキテクチャ設計
- `nari-note-backend/Documents/error-handling-strategy.md` - エラーハンドリング戦略
- `nari-note-backend/Documents/development-workflow.md` - 開発ワークフロー

**フロントエンド:**
- `nari-note-frontend/docs/implementation-guide.md` - Atomic Design実装ガイド（最重要）
- `nari-note-frontend/docs/architecture.md` - アーキテクチャ設計
- `nari-note-frontend/docs/api-usage.md` - API使用方法

**共通:**
- `.github/copilot-instructions.md` - GitHub Copilot向けの指示（コミットメッセージ規約等）

## コミットメッセージ規約

形式: `Prefix: {簡潔な変更内容（日本語で）}`

**Prefix:**
- `Feat`: 新機能の追加
- `Fix`: バグ修正
- `Refactor`: リファクタリング（機能変更を伴わない）
- `Chore`: ビルドプロセスやツールの変更、雑務
- `Docs`: ドキュメントのみの変更

**例:**
```
Feat: 記事作成機能を追加
Fix: ログイン時のエラーハンドリングを修正
Refactor: Repository層のクエリロジックを整理
```

## その他の注意事項

- レビューやコードの説明は**すべて日本語**で行う
- バックエンドのポート: 5243
- フロントエンドのポート: 3000
- PostgreSQLのポート: 5432
- Docker Compose使用時はサービス名でアクセス（例: `db`, `api`, `web`）
