# nari-note-backend

nari-note の API サーバー（ASP.NET Core Web API / .NET 9、PostgreSQL 17 + EF Core）。技術スタックの一覧はルートの [AGENTS.md](../AGENTS.md) を参照。

## 起動

リポジトリルートで Docker Compose を使う。API は `http://localhost:5243` で起動し、起動時にマイグレーションとシードデータの投入を行う。

```bash
docker compose up -d
```

ビルドとテスト:

```bash
cd nari-note-backend
dotnet build
dotnet test   # 結合テストは Docker が必要
```

詳細は [development-workflow.md](./Documents/development-workflow.md)。

## 構成

| ディレクトリ | 内容 |
|---|---|
| `Src/Controller` | API エンドポイント（API 一覧の正） |
| `Src/Application` | Service（API 一個につき一個）、DTO、カスタム例外 |
| `Src/Domain` | Entity、Repository / Gateway の interface、ID の ValueObject |
| `Src/Infrastructure` | DbContext、Repository 実装、外部サービス連携、バックグラウンドジョブ |
| `Src/Middleware`, `Src/Filter` | 例外ハンドラー・JWT 認証・トランザクション、認証レベル等の属性 |
| `Migrations` | EF Core マイグレーション（スキーマの正はスナップショット） |
| `Tests` | 単体テスト・結合テスト |

## ドキュメント

| ドキュメント | 内容 |
|---|---|
| [architecture.md](./Documents/architecture.md) | レイヤー構成、リクエストパイプライン、エラーハンドリング、バックグラウンドジョブ |
| [backend-implementation-guide.md](./Documents/backend-implementation-guide.md) | コーディング規約、層ごとのルール、新規 API の追加手順 |
| [authentication-strategy.md](./Documents/authentication-strategy.md) | Cookie JWT、メール認証、パスワード再設定、トークン失効 |
| [er-diagram.md](./Documents/er-diagram.md) | ER 図 |
| [testing-guide.md](./Documents/testing-guide.md) | テストの方針と書き方 |
| [development-workflow.md](./Documents/development-workflow.md) | ローカル開発、マイグレーション、設定キー |
