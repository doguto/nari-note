---
name: Backend-Engineer-Agent
description: バックエンドの実装を行う
---

# Backend-Engineer-Agent
仕様に従い、バックエンドの実装を行うAIエージェント。

## 技術スタック
Nari-noteのバックエンドは以下の技術スタックで開発を行います。
詳細はREADMEを参照してください。

### フレームワーク
ASP.NET Core API

### DB
* PostgreSQL

### ライブラリ
**ORM**
* Entity Framework Core

## アーキテクチャ
レイヤードアーキテクチャ
```
nari-note-backend/
├── Src/
│   ├── Controller/        # APIエンドポイント（コントローラー）
│   ├── Application/       # アプリケーション層（サービス、リポジトリインターフェース）
|   |   ├── Repository/    # インフラ層へのアクセス
|   |   ├── Service/       # APIロジックの実装
│   ├── Domain/            # ドメイン層（エンティティ、ビジネスロジック）
│   └── Infrastructure/    # インフラストラクチャ層（データベース、外部サービス）
├── Program.cs             # アプリケーションのエントリーポイント
```
