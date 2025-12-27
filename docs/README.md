# ドキュメント一覧

このディレクトリにはnari-noteプロジェクトの設計ドキュメントが格納されています。

## アーキテクチャ関連

### [api-reference.md](./api-reference.md)
**APIエンドポイントの完全なリファレンス**

**内容:**
- 全エンドポイントの仕様
- リクエスト/レスポンス例
- エラーレスポンス形式
- curl例

**API仕様を確認する際は、このドキュメントを参照してください。**

### [architecture.md](./architecture.md)
バックエンドの基本アーキテクチャ設計
- レイヤー構成（Controller、Application、Domain、Infrastructure）
- Entity Framework Coreとの統合
- Service粒度の設計方針（API一個につきService一個）

### [backend-implementation-guide.md](./backend-implementation-guide.md) ⭐ 重要
**バックエンド実装の包括的ガイド**

このドキュメントは、AI（GitHub Copilot等）が一貫した実装を行えるように設計されています。

**内容:**
- コーディング規約
  - 命名規則（private変数、this.の使用）
  - アクセス修飾子の規約
  - 日付時刻の扱い
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

### [development-workflow.md](./development-workflow.md)
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

### [er-diagram.md](./er-diagram.md)
データベースのER図とエンティティ設計

## エラーハンドリング

### [error-handling-strategy.md](./error-handling-strategy.md) ⭐ 重要
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

### [error-handling-examples.md](./error-handling-examples.md)
**エラーハンドリングの実装例とクイックリファレンス**

**内容:**
- 実装済みファイルの一覧
- Service、Repository、Controllerでの使用例
- エラーレスポンス例
- テスト方法

**簡単な実装例を確認したい場合は、このドキュメントを参照してください。**

## ドキュメントの使い方

### 新規機能を実装する場合
1. `backend-implementation-guide.md` で実装パターンとコーディング規約を確認 ⭐
2. `development-workflow.md` で具体的な開発手順を確認 ⭐
3. `architecture.md` でアーキテクチャの全体像を確認
4. `error-handling-strategy.md` でエラーハンドリング方針を確認
5. `error-handling-examples.md` で具体的な実装例を参照

### エラーハンドリングを実装する場合
1. `error-handling-strategy.md` で詳細な戦略を確認
2. `error-handling-examples.md` でクイックリファレンスを参照

### データベース設計を確認する場合
1. `er-diagram.md` でエンティティ関係を確認

### API仕様を確認する場合
1. `api-reference.md` で全エンドポイントの仕様を確認

### コーディング規約を確認する場合
1. `backend-implementation-guide.md` のコーディング規約セクションを確認

## AI（GitHub Copilot）向けの情報

これらのドキュメントは、AI開発支援ツールが自動的に参照し、コード生成やレビューに活用できるように構造化されています。

**特に重要なドキュメント:**
- `backend-implementation-guide.md` - 実装パターンとコーディング規約の完全なガイド ⭐
- `development-workflow.md` - 開発ワークフローと実践的タスクガイド ⭐
- `error-handling-strategy.md` - エラーハンドリングの完全なガイド
- `api-reference.md` - APIエンドポイントの完全なリファレンス
- `architecture.md` - アーキテクチャの基本方針

**実装の優先順位:**
1. まず `backend-implementation-guide.md` で全体像とコーディング規約を把握
2. 次に `development-workflow.md` で具体的な開発手順を理解
3. `error-handling-strategy.md` でエラーハンドリングを理解
4. 必要に応じて `api-reference.md` で既存のAPI仕様を確認
5. 必要に応じて `architecture.md` で設計思想を確認
