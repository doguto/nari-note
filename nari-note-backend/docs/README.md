# バックエンドドキュメント

このディレクトリには、nari-note-backendの設計・実装ドキュメントが格納されています。

## ドキュメント一覧

### 📚 実装ガイド

#### [backend-implementation-guide.md](./backend-implementation-guide.md) ⭐ 最重要
**バックエンド実装の包括的ガイド**

このドキュメントは、AI（GitHub Copilot等）が一貫した実装を行えるように設計されています。

**内容:**
- コーディング規約
  - 命名規則（private修飾子の省略、アンダースコアなし）
  - アクセス修飾子の規約
  - 日付時刻の扱い（DateTime.UtcNow）
- レイヤー別実装パターン
  - Controller層の実装パターンとルール
  - Service層の実装パターンとルール（API一個につきService一個）
  - Repository層の実装パターンとルール
  - Domain層（Entity）の実装パターン
- DTO設計パターン
  - Request DTO設計
  - Response DTO設計
- 開発ワークフロー
  - 新規機能追加の手順

**新機能を実装する際は、まずこのドキュメントを参照してください。**

### 🏗️ アーキテクチャ

#### [architecture-overview.md](./architecture-overview.md)
**アーキテクチャ概要（実装用）**

実装時にすぐに参照できるアーキテクチャの概要です。

**内容:**
- レイヤー構成
- ディレクトリ構成
- 各レイヤーの責務
- 重要な設計原則
- データフロー例

#### [architecture.md](./architecture.md)
**アーキテクチャ詳細（設計思想）**

アーキテクチャの選定理由や設計思想を説明します。

**内容:**
- 基本アーキテクチャの選定理由
- 安定要素と不安定要素の整理
- レイヤー構成の詳細
- Entity Framework Coreとの統合
- Service粒度の設計方針（API一個につきService一個）

### 🔨 開発ワークフロー

#### [development-workflow.md](./development-workflow.md)
**開発ワークフローと実践的なタスクガイド**

**内容:**
- 環境セットアップ手順
- 新規APIエンドポイントの追加方法（実例付き）
- データベーススキーマの変更手順
- マイグレーション管理
- デバッグとトラブルシューティング
- よくあるエラーと解決方法
- 便利なコマンド集

**実際の開発作業を行う際は、このドキュメントを参照してください。**

### 🗄️ データベース設計

#### [er-diagram.md](./er-diagram.md)
**データベースのER図とエンティティ設計**

**内容:**
- ER図（Mermaid形式）
- 各テーブルの定義
- インデックス・制約

### ⚠️ エラーハンドリング

#### [error-handling-strategy.md](./error-handling-strategy.md) ⭐ 重要
**エラーハンドリングの包括的な戦略ドキュメント**

このドキュメントは、AI（GitHub Copilot等）が自動参照し、実装に反映できるように設計されています。

**内容:**
- エラーハンドリングの基本方針
- カスタム例外クラスの設計
  - ApplicationException基底クラス
  - NotFoundException (404)
  - ValidationException (400)
  - ConflictException (409)
  - UnauthorizedException (401)
  - ForbiddenException (403)
  - InfrastructureException (500)
- グローバル例外ハンドラーミドルウェア
- エラーレスポンス形式
- レイヤー別の実装ガイドライン
- Sentry連携方法
- ベストプラクティス

**新機能を実装する際は、このドキュメントに従ってエラーハンドリングを実装してください。**

### 🔐 認証

#### [authentication-strategy.md](./authentication-strategy.md)
**認証戦略**

**内容:**
- 認証方式（JWT）
- トークン管理
- ミドルウェア実装
- セキュリティ考慮事項

## ドキュメントの使い方

### 新規機能を実装する場合
1. **[backend-implementation-guide.md](./backend-implementation-guide.md)** で実装パターンとコーディング規約を確認 ⭐
2. **[development-workflow.md](./development-workflow.md)** で具体的な開発手順を確認 ⭐
3. **[architecture-overview.md](./architecture-overview.md)** でアーキテクチャの全体像を確認
4. **[error-handling-strategy.md](./error-handling-strategy.md)** でエラーハンドリング方針を確認
5. **[er-diagram.md](./er-diagram.md)** でデータベース設計を確認

### エラーハンドリングを実装する場合
1. **[error-handling-strategy.md](./error-handling-strategy.md)** で詳細な戦略を確認

### データベース設計を確認する場合
1. **[er-diagram.md](./er-diagram.md)** でER図とエンティティ関係を確認

### コーディング規約を確認する場合
1. **[backend-implementation-guide.md](./backend-implementation-guide.md)** のコーディング規約セクションを確認

### 認証を実装する場合
1. **[authentication-strategy.md](./authentication-strategy.md)** で認証戦略を確認

## AI（GitHub Copilot）向けの情報

これらのドキュメントは、AI開発支援ツールが自動的に参照し、コード生成やレビューに活用できるように構造化されています。

**特に重要なドキュメント:**
- **[backend-implementation-guide.md](./backend-implementation-guide.md)** - 実装パターンとコーディング規約の完全なガイド ⭐
- **[development-workflow.md](./development-workflow.md)** - 開発ワークフローと実践的タスクガイド ⭐
- **[error-handling-strategy.md](./error-handling-strategy.md)** - エラーハンドリングの完全なガイド
- **[architecture-overview.md](./architecture-overview.md)** - アーキテクチャの基本方針

**実装の優先順位:**
1. まず **[backend-implementation-guide.md](./backend-implementation-guide.md)** で全体像とコーディング規約を把握
2. 次に **[development-workflow.md](./development-workflow.md)** で具体的な開発手順を理解
3. **[error-handling-strategy.md](./error-handling-strategy.md)** でエラーハンドリングを理解
4. 必要に応じて **[architecture.md](./architecture.md)** で設計思想を確認

## 関連リンク

- [フロントエンドドキュメント](../../nari-note-frontend/docs/README.md)
- [プロジェクトルートREADME](../../README.md)
