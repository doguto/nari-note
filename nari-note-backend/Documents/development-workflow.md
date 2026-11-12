# 開発ワークフロー

ローカル開発環境、マイグレーション、設定キーをまとめる。本番環境の構成とデプロイ手順は [infrastruture/docs/infra-design.md](../../infrastruture/docs/infra-design.md) を参照。

## ローカル起動

リポジトリルートの `docker-compose.yml` で DB・API・フロントエンドをまとめて起動する。

```bash
# リポジトリルートで
docker compose up -d
docker compose logs -f api
```

| サービス | 内容 | ポート |
|---|---|---|
| `db` | PostgreSQL 17（`pg_stat_statements` 有効） | 5432 |
| `api` | `dotnet watch run` によるホットリロード（Development 環境） | 5243 |
| `web` | Next.js の開発サーバー | 3000 |

- API は起動時にマイグレーションを適用し、Development では開発用のシードデータを投入する。シードユーザーのアカウントは `Src/Infrastructure/Database/DataSeeder.cs` を参照
- Development では画像ストレージがダミー（`LocalImageStorageGateway`。アップロードせずダミー URL を返す）になる
- DB に直接入る: `docker compose exec db psql -U myuser -d mydatabase`
- DB を作り直す: `docker compose down -v`（ボリュームごと削除）してから起動し直す

ホストで直接 API を動かす場合は、DB だけコンテナで起動して `nari-note-backend` で `dotnet run` する（`appsettings.Development.json` の接続文字列は `localhost:5432` を向いている）。

## ビルドとテスト

```bash
cd nari-note-backend
dotnet build
dotnet test
```

テストの詳細は [testing-guide.md](./testing-guide.md)。

## マイグレーション

EF Core のツールはローカルツールとして `nari-note-backend/.config/dotnet-tools.json` に定義している。

```bash
cd nari-note-backend
dotnet tool restore                        # 初回のみ

dotnet ef migrations add <マイグレーション名>  # Entity / DbContext の変更から作成
dotnet ef migrations remove                # 直前のマイグレーションを取り消す（未適用の場合）
```

- マイグレーションは全環境で API の起動時に自動適用される。手動で `dotnet ef database update` する必要は基本的に無い
- 生成されたマイグレーションと `Migrations/NariNoteDbContextModelSnapshot.cs` は必ずレビューしてコミットする（スナップショットがスキーマの正）
- 既存データの変換を伴うマイグレーションは、`Tests/NariNoteBackend.Tests/Migrations/` にテストを追加する
- 本番では起動時に適用されるため、大きなテーブルのロックや長時間かかる変更は避けるか、デプロイ手順を別途検討する

## 設定キー

設定は以下の順で読み込まれる（後のものが優先）。

1. `appsettings.json` → `appsettings.{環境名}.json`
2. 環境変数（例: `ConnectionStrings__DefaultConnection`）
3. Development のみ: `secret.json`（git 管理外。外部サービスのトークン等の開発用シークレットを置く）
4. Development / Testing 以外: AWS SSM Parameter Store の `/{APP_NAME}/app` と `/{APP_NAME}/db`（`APP_NAME` 環境変数、未指定時は `nari-note`）。本番の値はここで上書きされる

`secret.json` と SSM は `Program.cs` で後から追加するため、環境変数より優先される。

値はここには書かない。キー名と用途のみ示す。

| キー | 用途 |
|---|---|
| `ConnectionStrings:DefaultConnection` | DB 接続文字列。未設定の場合は下の個別キーから組み立てる |
| `host` / `port` / `name` / `username` / `password` | DB 接続情報（SSM の `/db` から読む） |
| `Jwt:Secret` / `Jwt:Issuer` / `Jwt:Audience` | JWT の署名鍵・発行者・対象 |
| `Jwt:ExpirationInHours` | JWT と認証 Cookie の有効期間（時間） |
| `Frontend:BaseUrl` | メール本文に載せるフロントエンドの URL（必須） |
| `Cors:AllowedOrigins` | CORS で許可するオリジン |
| `resend_api_token` | メール送信（Resend）の API トークン |
| `discord_general_webhook_url` | Discord 通知の Webhook URL |
| `image_delivery_bucket_name` / `image_delivery_domain` | 画像を保存する S3 バケットと配信ドメイン（Development 以外） |
| `sentry_dsn` | Sentry の DSN |
| `Serilog` | ログ出力の設定（本番はファイルにも出力） |
