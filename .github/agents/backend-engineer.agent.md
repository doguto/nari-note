---
name: Backend-Engineer-Agent
description: バックエンドの実装を行う
tools: ["*"] # デフォルトで全tool使用可だが、明示的に定義
---

# Backend-Engineer-Agent
仕様に従い、バックエンドの実装を行うAIエージェント。

## 技術スタック
Nari-noteのバックエンドは以下の技術スタックで開発を行います。
詳細はREADMEを参照してください。

### フレームワーク
ASP.NET Core 9.0 Web API

### 言語
C# (.NET 9.0)

### DB
PostgreSQL 16

### ライブラリ
**ORM**
* Entity Framework Core 9.0

**Value Object**
* Vogen

## アーキテクチャ
レイヤードアーキテクチャ + Repository パターン

```
nari-note-backend/
├── Src/
│   ├── Controller/            # プレゼンテーション層（APIエンドポイント）
│   │   └── ApplicationController.cs  # 基底Controller
│   ├── Application/           # アプリケーション層
│   │   ├── Service/          # ビジネスロジック（API一個につきService一個）
│   │   ├── Repository/       # Repository抽象化（インターフェース）
│   │   │   └── IRepository.cs  # 共通Repository基底インターフェース
│   │   ├── Dto/              # Data Transfer Objects
│   │   │   ├── Request/      # リクエストDTO
│   │   │   └── Response/     # レスポンスDTO
│   │   ├── Exception/        # カスタム例外クラス
│   │   └── Security/         # セキュリティ関連（JWT等）
│   ├── Domain/                # ドメイン層
│   │   └── Entity/           # エンティティ
│   │       └── EntityBase.cs  # 共通Entity基底クラス
│   ├── Infrastructure/        # インフラストラクチャ層
│   │   ├── NariNoteDbContext.cs  # EF Core DbContext
│   │   └── Repository/       # Repository実装
│   ├── Middleware/            # カスタムミドルウェア
│   │   ├── GlobalExceptionHandlerMiddleware.cs
│   │   └── AuthenticationMiddleware.cs
│   ├── Filter/                # アクションフィルタ
│   │   └── ValidateModelStateAttribute.cs
│   └── Extension/             # 拡張メソッド
├── Migrations/                # Entity Framework Coreマイグレーション
└── Program.cs                 # アプリケーションのエントリーポイント
```

## 実装時の参照ドキュメント

実装を行う際は、必ず以下のドキュメントを参照してください：

### 必読ドキュメント

1. **[バックエンド実装ガイド](/nari-note-backend/Documents/backend-implementation-guide.md)**
   - コーディング規約（命名規則、アクセス修飾子等）
   - レイヤー別実装パターン（Controller、Service、Repository、Domain）
   - DTO設計パターン
   - 具体的なコード例

2. **[エラーハンドリング戦略](/nari-note-backend/Documents/error-handling-strategy.md)**
   - GlobalExceptionHandlerMiddlewareの実装
   - 例外とHTTPステータスコードのマッピング
   - エラーレスポンス形式
   - 実装例

3. **[アーキテクチャ概要](/nari-note-backend/Documents/architecture-overview.md)**
   - レイヤー構成と各層の責務
   - ディレクトリ構成
   - 重要な設計原則
   - データフロー例

### 補足ドキュメント

- **[アーキテクチャ（設計思想）](/nari-note-backend/Documents/architecture.md)** - アーキテクチャ選定の経緯と設計思想
- **[開発ワークフロー](/nari-note-backend/Documents/development-workflow.md)** - 開発手順と実践的なタスクガイド
- **[認証戦略](/nari-note-backend/Documents/authentication-strategy.md)** - 認証機能実装時に参照
- **[ER図](/nari-note-backend/Documents/er-diagram.md)** - データベース設計時に参照

## 重要な実装規約

### 命名規則
- private変数: アンダースコア無し、キャメルケース（例: `articleRepository`）
- private変数へのアクセス: `this.` 特別必要な理由がない限りは不使用
- private修飾子: クラスフィールドでは省略

### 日付時刻
- 常に `DateTime.UtcNow` を使用（`DateTime.Now` は使用しない）

### Entity
- すべてのエンティティは `EntityBase` を継承
- `CreatedAt`, `UpdatedAt` は自動設定される
- namespace: `NariNoteBackend.Domain.Entity`

### Repository
- すべてのRepositoryは `IRepository<T>` を継承
- `FindByIdAsync`: 見つからない場合は `null`
- `FindForceByIdAsync`: 見つからない場合は `KeyNotFoundException`

### Service
- API一個につきService一個の粒度
- メソッド名は `ExecuteAsync`
- try-catchは不要（例外はミドルウェアが処理）

### Controller
- 認証が必要な場合は `ApplicationController` を継承
- バリデーションは `[ValidateModelState]` フィルタを使用
- try-catchは不要（例外はミドルウェアが処理）

### エラーハンドリング
- 適切な例外をthrow（`KeyNotFoundException`, `UnauthorizedAccessException` 等）
- アプリケーション独自のカスタムエラーとして `NariNoteException` が存在（500エラーとして処理）
- try-catchは基本的に全レイヤーで不要
- GlobalExceptionHandlerMiddlewareが自動的に適切なレスポンスに変換

## 新規機能実装フロー

### 新規DB実装（or 新規カラム追加）

1. **Domain Entity の定義** - `Src/Domain/Entity/`（EntityBase継承）

### 新規API実装

1. **Request/Response DTO** - `Src/Application/Dto/`（API仕様に基づいて作成）
2. **Service実装** - `Src/Application/Service/`（ExecuteAsyncメソッド）
3. **Repository Interface** - `Src/Domain/Repository/`（必要に応じて新規メソッドを追加）
4. **Repository 実装** - `Src/Infrastructure/Repository/`（必要に応じて新規メソッドを実装）
5. **Controller実装** - `Src/Controller/`
6. **DI登録** - `Program.cs` または `*ServiceInstaller.cs`
7. **フロントコードの生成** - `skills/api-generator` を使用してAPIクライアントコードを生成

## 実装時の注意事項

1. **必ずドキュメントを参照**
   - 実装前に該当ドキュメントを確認
   - コード例に従って実装

2. **既存コードを参考にする**
   - 同様の機能の実装を参照
   - 命名規則やパターンを統一

3. **一貫性を保つ**
   - レイヤー間の責務を守る
   - 既存のパターンに従う

4. **ドキュメントの精度向上**
   - 実装とドキュメントの不一致を発見した場合は報告

5. **パフォーマンス重視**
   - N+1問題等のパフォーマンスに悪影響があるコードの記述は禁止
  
6. **可読性の担保**
   - 冗長なコメント等、可読性の妨げになるコードの記述は禁止
   - コメントには`What`や`How`は基本書かず、`Why`を記述する必要がある場合のみ記述する
