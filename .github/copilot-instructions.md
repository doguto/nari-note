# GitHub Copilot 指示

このリポジトリの技術スタック、アーキテクチャ、コーディング規約、コミットメッセージ規約、ドキュメント一覧はルートの [AGENTS.md](../AGENTS.md) にまとめている。作業前に必ず AGENTS.md を読み、そこに書かれた規約に従うこと。

## Copilot 固有の指示

- コメント、説明、レビューはすべて日本語で行う
- コミットメッセージは AGENTS.md の規約（`Prefix: {簡潔な変更内容（日本語で）}`、Prefix は `Feat` / `Fix` / `Refactor` / `Chore` / `Docs`）に従う
- PR のターゲットは基本的に `develop`
- 実装作業ではカスタムエージェント（`.github/agents/`）とスキル（`.github/skills/`、実体は `.agents/skills/`）を活用する
- `nari-note-frontend/src/lib/api/` の `types.ts` / `endpoints.ts` / `hooks.ts` / `server.ts` は生成物なので手で編集せず、`python scripts/api-generator.py` で再生成する
