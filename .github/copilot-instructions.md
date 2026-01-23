# GitHub Copilot 指示

## ドキュメント参照

プロジェクトのドキュメントは以下の場所に整理されています：

### バックエンド（ASP.NET Core）
- **場所**: `nari-note-backend/Documents/`
- **主要ドキュメント**:
  - [backend-implementation-guide.md](../nari-note-backend/Documents/backend-implementation-guide.md) - 実装パターンとコーディング規約（最重要）
  - [architecture.md](../nari-note-backend/Documents/architecture.md) - アーキテクチャ設計
  - [development-workflow.md](../nari-note-backend/Documents/development-workflow.md) - 開発ワークフロー
  - [error-handling-strategy.md](../nari-note-backend/Documents/error-handling-strategy.md) - エラーハンドリング戦略

### フロントエンド（Next.js）
- **場所**: `nari-note-frontend/docs/`
- **主要ドキュメント**:
  - [implementation-guide.md](../nari-note-frontend/docs/implementation-guide.md) - Atomic Design実装ガイド（最重要）
  - [architecture.md](../nari-note-frontend/docs/architecture.md) - アーキテクチャ設計
  - [frontend-api-usage.md](../nari-note-frontend/docs/frontend-api-usage.md) - API使用方法

### プロジェクト共通
- **場所**: `docs/`
- [README.md](../docs/README.md) - プロジェクト全体の概要とナビゲーション

## レビュー

- レビューコメントやフィードバックは全て日本語で行ってください
- コードの説明や提案も日本語で記述してください

## コミットメッセージ

コミットメッセージは以下の形式に従ってください：

```
Prefix: {簡潔な変更内容（日本語で）}
```

### 使用可能なPrefix

- **Feat**: 新機能の追加
- **Fix**: バグ修正
- **Refactor**: リファクタリング（機能変更を伴わないコード改善）
- **Chore**: ビルドプロセスやツールの変更、雑務
- **Docs**: ドキュメントのみの変更

### コミットメッセージの例

```
Feat: ユーザー認証機能を追加
Fix: ログイン時のエラーハンドリングを修正
Refactor: データベース接続ロジックを整理
Chore: 依存パッケージを最新版に更新
Docs: READMEにセットアップ手順を追加
```

## コーディング規約

### C#

#### 命名規則

- **private変数**: アンダースコア（`_`）のプレフィックスを付けずに、キャメルケースで命名してください
  - 良い例: `readonly NariNoteDbContext context;`
  - 悪い例: `readonly NariNoteDbContext _context;`
  - ※例では`private`修飾子を省略しています
- private変数へのアクセスには `this.` を使用してください
  - 例: `this.context.SaveChangesAsync();`

#### アクセス修飾子

- **private修飾子の省略**: クラスのフィールドにおいて、`private`修飾子は省略してください（C#ではデフォルトでprivateになります）
  - 良い例: `readonly NariNoteDbContext context;`
  - 悪い例: `private readonly NariNoteDbContext context;`

#### ValueObject（ID型）

- **Vogenの使用**: エンティティのIDには、Vogenライブラリを使用した型安全なValueObjectを使用してください
  - 例: `ArticleId`, `UserId`, `TagId` など
  - ValueObjectは `NariNoteBackend.Domain.ValueObject` 名前空間に定義されています
