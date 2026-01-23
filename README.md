# nari-note

nari-noteは、将棋記事の投稿・共有プラットフォームです。

## プロジェクト構成

このプロジェクトは以下のコンポーネントで構成されています：

- **nari-note-backend** - ASP.NET Core Web API（バックエンド）
  - [バックエンドREADME](./nari-note-backend/README.md)
  - [バックエンドドキュメント](./nari-note-backend/docs/README.md)
- **nari-note-frontend** - Next.js（フロントエンド）
  - [フロントエンドREADME](./nari-note-frontend/README.md)
  - [フロントエンドドキュメント](./nari-note-frontend/docs/README.md)
- **docs** - プロジェクト共通ドキュメント
  - [プロジェクトドキュメント](./docs/README.md)

## 技術スタック

### バックエンド
- **フレームワーク**: ASP.NET Core 9.0
- **言語**: C# (.NET 9.0)
- **データベース**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0

### フロントエンド
- **フレームワーク**: Next.js 15
- **言語**: TypeScript
- **UIライブラリ**: React 19
- **データフェッチング**: TanStack Query (React Query)
- **スタイリング**: Tailwind CSS 4

## ドキュメント

### 📚 ドキュメント構成

プロジェクトのドキュメントは、以下のように整理されています：

- **[バックエンドドキュメント](./nari-note-backend/docs/README.md)** - バックエンド（ASP.NET Core）に関する全てのドキュメント
- **[フロントエンドドキュメント](./nari-note-frontend/docs/README.md)** - フロントエンド（Next.js）に関する全てのドキュメント
- **[プロジェクトドキュメント](./docs/README.md)** - プロジェクト全体に関わる共通のドキュメント

### クイックスタート

#### バックエンド開発を始める
1. [バックエンドドキュメント](./nari-note-backend/docs/README.md) を読む
2. [backend-implementation-guide.md](./nari-note-backend/docs/backend-implementation-guide.md) で実装パターンを確認
3. [development-workflow.md](./nari-note-backend/docs/development-workflow.md) で開発手順を確認

#### フロントエンド開発を始める
1. [フロントエンドドキュメント](./nari-note-frontend/docs/README.md) を読む
2. [frontend-implementation-guide.md](./nari-note-frontend/docs/frontend-implementation-guide.md) でAtomic Designパターンを確認
3. [frontend-api-usage.md](./nari-note-frontend/docs/frontend-api-usage.md) でAPI使用方法を確認

## 環境構築

### Docker Compose を使用（推奨）

```bash
# プロジェクトルートから
docker-compose up

# バックグラウンドで起動
docker-compose up -d
```

### ローカル環境

詳細は各コンポーネントのREADMEを参照してください：
- [バックエンド環境構築](./nari-note-backend/README.md)
- [フロントエンド環境構築](./nari-note-frontend/README.md)
