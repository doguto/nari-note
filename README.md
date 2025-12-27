# nari-note

nari-noteは、技術記事の投稿・共有プラットフォームです。

## プロジェクト構成

このプロジェクトは以下の3つのコンポーネントで構成されています：

- **nari-note-backend** - ASP.NET Core Web API（バックエンド）
- **nari-note-frontend** - Next.js（フロントエンド）
- **docs** - プロジェクトドキュメント

## 技術スタック

### バックエンド
- **フレームワーク**: ASP.NET Core 9.0
- **言語**: C# (.NET 9.0)
- **データベース**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0

### フロントエンド
- **フレームワーク**: Next.js
- **言語**: TypeScript

## クイックスタート

### Docker Composeを使用する場合（推奨）

```bash
# プロジェクトルートから
docker-compose up
```

- バックエンド: `http://localhost:5243`
- フロントエンド: `http://localhost:3000`
- PostgreSQL: `localhost:5432`

### ローカル環境で実行する場合

詳細は各コンポーネントのREADMEを参照してください：
- [nari-note-backend/README.md](./nari-note-backend/README.md)
- [nari-note-frontend/README.md](./nari-note-frontend/README.md)

## ドキュメント

プロジェクトの詳細なドキュメントは [docs](./docs) ディレクトリにあります。

### バックエンド開発者向け
- [バックエンド実装ガイド](./docs/backend-implementation-guide.md) ⭐
- [開発ワークフロー](./docs/development-workflow.md) ⭐
- [テスト実装ガイド](./docs/testing-guide.md) ⭐
- [アーキテクチャ](./docs/architecture.md)
- [エラーハンドリング戦略](./docs/error-handling-strategy.md)
- [APIリファレンス](./docs/api-reference.md)
- [ER図](./docs/er-diagram.md)

### すべてのドキュメント
[docs/README.md](./docs/README.md) でドキュメント一覧と使い方を確認できます。

## 主な機能

- ユーザー登録・ログイン
- 記事の投稿・編集・削除
- タグによる記事の分類
- 記事へのいいね機能
- ユーザーフォロー機能
- 通知機能

## 開発チーム

個人開発プロジェクトです。

## ライセンス

このプロジェクトのライセンスは未定です。
