# nari-note-backend

nari-noteプロジェクトのバックエンドAPIサーバーです。

## 技術スタック

- **フレームワーク**: ASP.NET Core 9.0
- **言語**: C# (.NET 9.0)
- **データベース**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0
- **アーキテクチャ**: レイヤードアーキテクチャ（Controller、Application、Domain、Infrastructure）

## 必要な環境

- .NET 9.0 SDK
- Docker & Docker Compose（推奨）
- PostgreSQL 16（ローカル実行の場合）

## セットアップ

### Docker Composeを使用する場合（推奨）

プロジェクトルートディレクトリから以下のコマンドを実行してください：

```bash
docker-compose up
```

バックエンドAPIサーバーは `http://localhost:5243` で起動します。

### ローカル環境で直接実行する場合

1. PostgreSQLをインストールし、起動してください

2. 依存パッケージをインストール：

```bash
cd nari-note-backend
dotnet restore
```

3. 開発サーバーを起動：

```bash
dotnet run
```

または、ホットリロードを有効にして起動：

```bash
dotnet watch run
```

## プロジェクト構成

```
nari-note-backend/
├── Src/
│   ├── Controller/        # APIエンドポイント（コントローラー）
│   ├── Application/        # アプリケーション層（サービス、リポジトリインターフェース）
│   ├── Domain/            # ドメイン層（エンティティ、ビジネスロジック）
│   └── Infrastructure/    # インフラストラクチャ層（データベース、外部サービス）
├── Program.cs             # アプリケーションのエントリーポイント
├── appsettings.json       # アプリケーション設定
├── Dockerfile             # Docker設定
└── nari-note-backend.csproj # プロジェクトファイル
```

## API エンドポイント

### ヘルスチェック

- `GET /health` - アプリケーションの健全性チェック
- `GET /api/Health` - APIの健全性チェック

## 開発

### パッケージの追加

```bash
dotnet add package <パッケージ名>
```

### マイグレーション（Entity Framework Core）

```bash
# マイグレーションの作成
dotnet ef migrations add <マイグレーション名>

# マイグレーションの適用
dotnet ef database update
```

### ビルド

```bash
dotnet build
```

### テスト実行

```bash
dotnet test
```

## 環境変数

| 変数名 | 説明 | デフォルト値 |
|--------|------|------------|
| DATABASE_URL | PostgreSQL接続URL | - |

## その他

- ポート: 5243
- HTTPS リダイレクトが有効化されています
- ヘルスチェックエンドポイントが `/health` で利用可能です
