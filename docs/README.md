# ドキュメント一覧

このディレクトリにはnari-noteプロジェクトの設計ドキュメントが格納されています。

## アーキテクチャ関連

### [architecture.md](./architecture.md)
バックエンドの基本アーキテクチャ設計
- レイヤー構成（Controller、Application、Domain、Infrastructure）
- Entity Framework Coreとの統合
- Service粒度の設計方針

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
1. `architecture.md` でアーキテクチャの全体像を確認
2. `error-handling-strategy.md` でエラーハンドリング方針を確認
3. `error-handling-examples.md` で具体的な実装例を参照

### エラーハンドリングを実装する場合
1. `error-handling-strategy.md` で詳細な戦略を確認
2. `error-handling-examples.md` でクイックリファレンスを参照

### データベース設計を確認する場合
1. `er-diagram.md` でエンティティ関係を確認

## AI（GitHub Copilot）向けの情報

これらのドキュメントは、AI開発支援ツールが自動的に参照し、コード生成やレビューに活用できるように構造化されています。

**特に重要なドキュメント:**
- `error-handling-strategy.md` - エラーハンドリングの完全なガイド
- `architecture.md` - アーキテクチャの基本方針
