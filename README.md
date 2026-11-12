# nari-note

[![Frontend Deploy](https://github.com/doguto/nari-note/actions/workflows/cloudflare-deploy.yml/badge.svg)](https://github.com/doguto/nari-note/actions/workflows/cloudflare-deploy.yml)

nari-note は、将棋の記事を投稿・共有するプラットフォームです。棋譜や盤面を埋め込んだ記事を書き、講座としてまとめて公開できます。

## 構成

| ディレクトリ | 内容 |
| --- | --- |
| [nari-note-backend](./nari-note-backend/README.md) | ASP.NET Core Web API + PostgreSQL |
| [nari-note-frontend](./nari-note-frontend/README.md) | Next.js（Cloudflare Workers にデプロイ） |
| [infrastruture](./infrastruture/AGENTS.md) | AWS インフラ（Terraform + Terragrunt） |
| [spec](./spec/README.md) | 画面・機能の仕様 |

技術スタックと全体構成図は [AGENTS.md](./AGENTS.md) にまとめています。

## ローカル起動

Docker Compose で DB・API・フロントエンドをまとめて起動します。

```bash
docker compose up
```

| サービス | URL |
| --- | --- |
| フロントエンド | http://localhost:3000 |
| API | http://localhost:5243 |
| PostgreSQL | localhost:5432 |

起動時にマイグレーションとシードデータの投入が自動で行われます。個別に起動する方法や設定は各ディレクトリの README を参照してください。

## ドキュメント

- [AGENTS.md](./AGENTS.md) - 技術スタック、アーキテクチャ概要、規約、ドキュメント一覧（開発者・AI エージェント共通の正本）
- [バックエンド README](./nari-note-backend/README.md) / [バックエンド設計](./nari-note-backend/Documents/architecture.md)
- [フロントエンド README](./nari-note-frontend/README.md) / [フロントエンド設計](./nari-note-frontend/docs/architecture.md)
- [インフラ設計・デプロイフロー](./infrastruture/docs/infra-design.md)

ブランチは `develop` が Default で、PR は `develop` 向けに作成します。コミットメッセージ規約は AGENTS.md を参照してください。
