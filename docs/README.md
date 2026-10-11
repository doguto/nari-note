# nari-note プロジェクトドキュメント

このディレクトリには、nari-noteプロジェクト全体の概要ドキュメントが格納されています。

## ドキュメント構成

nari-noteプロジェクトのドキュメントは、以下のように整理されています：

### 📂 バックエンドドキュメント
**場所**: [`nari-note-backend/Documents/`](../nari-note-backend/Documents/)

バックエンド（ASP.NET Core Web API）に関する全てのドキュメント

- 実装ガイド
- アーキテクチャ設計
- エラーハンドリング戦略
- 開発ワークフロー
- データベース設計（ER図）
- 認証戦略

詳細は [バックエンドドキュメントREADME](../nari-note-backend/Documents/README.md) を参照してください。

### 📂 フロントエンドドキュメント
**場所**: [`nari-note-frontend/docs/`](../nari-note-frontend/docs/)

フロントエンド（Next.js）に関する全てのドキュメント

- 実装ガイド（Atomic Design）
- アーキテクチャ設計
- API使用方法
- コンポーネント生成パターン
- クイックリファレンス

詳細は [フロントエンドドキュメントREADME](../nari-note-frontend/docs/README.md) を参照してください。

### 📂 共通ドキュメント
**場所**: このディレクトリ（`docs/`）

プロジェクト全体に関わる共通のドキュメント

- プロジェクト概要
- 技術スタック
- 環境構築（Docker）

## プロジェクト概要

### 技術スタック

#### バックエンド
- **フレームワーク**: ASP.NET Core 9.0
- **言語**: C# (.NET 9.0)
- **データベース**: PostgreSQL 17
- **ORM**: Entity Framework Core 9.0

#### フロントエンド
- **フレームワーク**: Next.js 15
- **言語**: TypeScript
- **UIライブラリ**: React 19
- **データフェッチング**: TanStack Query (React Query)
- **スタイリング**: Tailwind CSS 4

### プロジェクト構成

```
nari-note/
├── nari-note-backend/          # バックエンド（ASP.NET Core）
│   ├── docs/                   # バックエンドドキュメント
│   ├── Src/                    # ソースコード
│   ├── Migrations/             # EF Core マイグレーション
│   └── Program.cs              # エントリーポイント
├── nari-note-frontend/         # フロントエンド（Next.js）
│   ├── docs/                   # フロントエンドドキュメント
│   ├── src/                    # ソースコード
│   └── public/                 # 静的ファイル
├── docs/                       # 共通ドキュメント
├── docker-compose.yml          # Docker構成
└── README.md                   # プロジェクトREADME
```

## クイックスタート

### バックエンドの開発を始める

1. [バックエンドドキュメント](../nari-note-backend/docs/README.md) を読む
2. [backend-implementation-guide.md](../nari-note-backend/docs/backend-implementation-guide.md) で実装パターンを確認
3. [development-workflow.md](../nari-note-backend/docs/development-workflow.md) で開発手順を確認

### フロントエンドの開発を始める

1. [フロントエンドドキュメント](../nari-note-frontend/docs/README.md) を読む
2. [implementation-guide.md](../nari-note-frontend/docs/implementation-guide.md) でAtomic Designパターンを確認
3. [api-usage.md](../nari-note-frontend/docs/api-usage.md) でAPI使用方法を確認

## 重要なドキュメント

### バックエンド開発者向け
- ⭐ [backend-implementation-guide.md](../nari-note-backend/Documents/backend-implementation-guide.md) - 実装パターンとコーディング規約
- ⭐ [development-workflow.md](../nari-note-backend/Documents/development-workflow.md) - 開発ワークフロー
- [error-handling-strategy.md](../nari-note-backend/Documents/error-handling-strategy.md) - エラーハンドリング戦略

### フロントエンド開発者向け
- ⭐ [implementation-guide.md](../nari-note-frontend/docs/implementation-guide.md) - Atomic Designとコンポーネント生成
- ⭐ [api-usage.md](../nari-note-frontend/docs/api-usage.md) - API使用方法
- [architecture.md](../nari-note-frontend/docs/architecture.md) - アーキテクチャガイド

## AI（GitHub Copilot）向けの情報

このプロジェクトのドキュメントは、AI開発支援ツールが自動的に参照し、コード生成やレビューに活用できるように構造化されています。

### バックエンド実装時
1. [backend-implementation-guide.md](../nari-note-backend/Documents/backend-implementation-guide.md) で全体像を把握
2. [development-workflow.md](../nari-note-backend/Documents/development-workflow.md) で開発手順を理解
3. [error-handling-strategy.md](../nari-note-backend/Documents/error-handling-strategy.md) でエラーハンドリングを確認

### フロントエンド実装時
1. [implementation-guide.md](../nari-note-frontend/docs/implementation-guide.md) でAtomic Designパターンを把握
2. [api-usage.md](../nari-note-frontend/docs/api-usage.md) でAPI使用方法を理解
3. [architecture.md](../nari-note-frontend/docs/architecture.md) でアーキテクチャを確認
