# nari-note-frontend

なりノート（将棋記事の投稿・共有プラットフォーム）のフロントエンド。Next.js（App Router）で実装し、Cloudflare Workers に OpenNext でデプロイする。

技術スタックはルートの [AGENTS.md](../AGENTS.md) を、依存バージョンは `package.json` を正とする。

## セットアップ

Node.js のバージョンは `.node-version` を正とする。

```bash
cd nari-note-frontend
cp .env.local.example .env.local   # 必要に応じて値を編集
npm install
npm run dev                         # http://localhost:3000
```

バックエンド（既定 `http://localhost:5243`）が起動している必要がある。ルートで `docker compose up` するとバックエンド・DB と一緒にフロントエンドも起動する（その場合 `API_URL` は compose 側で `http://api:5243` に設定される）。

## npm scripts

| コマンド | 用途 |
|---|---|
| `npm run dev` | 開発サーバー |
| `npm run build` | Next.js の本番ビルド（型チェック込み） |
| `npm run lint` | ESLint |
| `npm run pages:build` | Next.js ビルド + OpenNext で Cloudflare Workers 向けに変換（`.open-next/` に出力） |
| `npm run preview` | `pages:build` 後に `wrangler dev` で Workers ランタイム上で動作確認 |
| `npm run deploy` | `pages:build` 後に `wrangler deploy` |

## 環境変数

ローカルは `.env.local`（雛形は `.env.local.example`）、本番は `wrangler.toml` の `[vars]` と Cloudflare 側のシークレットで設定する。サーバー側の値は `utils/env.ts` の `getEnv()` 経由で読む（Workers の env を優先し、無ければ `process.env`）。

| キー | 用途 |
|---|---|
| `API_URL` | バックエンドの URL。BFF プロキシ（`app/api/[...path]`）とサーバー側 fetch（`lib/api/server.ts`）の転送先 |
| `INTERNAL_API_KEY` | バックエンド前段の nginx が検証する共有シークレット。`X-Narinote-Kf` ヘッダーで送る。ローカルでは空でよい |
| `NEXT_PUBLIC_IMAGE_HOSTNAME` | 画像 CDN のホスト名（`next/image` の許可リスト） |
| `NEXT_PUBLIC_SITE_URL` | サイトの絶対 URL（metadata・sitemap・robots の基準。未設定時は本番 URL） |

`wrangler.toml` の `WORKER_SELF_REFERENCE` はリンクプレビューで自サイトのページを取得するためのサービスバインディング。

## デプロイ

`main` へのマージで Cloudflare の Deploy Hook が起動しビルド・デプロイされる。詳細は [infrastruture/docs/infra-design.md](../infrastruture/docs/infra-design.md) を参照。

## ドキュメント

- [docs/architecture.md](./docs/architecture.md) - レイヤー構成、レンダリング方針、API 通信、認証状態、キャッシュ方針
- [docs/implementation-guide.md](./docs/implementation-guide.md) - ページ・機能の追加手順とコンポーネント規約
- [docs/api-usage.md](./docs/api-usage.md) - API コード生成（api-generator）と使い方
