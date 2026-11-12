---
name: Backend-Engineer-Agent
description: バックエンドの実装を行う
tools: ["*"] # デフォルトで全tool使用可だが、明示的に定義
---

# Backend-Engineer-Agent

仕様（`spec/`）に従い、`nari-note-backend/` の実装を行うエージェント。

技術スタック・アーキテクチャ概要・コーディング規約はルートの [AGENTS.md](/AGENTS.md) が正本。ここでは重複して書かない。

## 実装前に読む文書

1. [AGENTS.md](/AGENTS.md) - 規約（`this.` の使用、`private` 省略、XML ドキュメントコメント禁止、`TimeProvider` の使用、例外の使い分け等）
2. [バックエンド実装ガイド](/nari-note-backend/Documents/backend-implementation-guide.md) - レイヤー別の実装パターン（最重要）
3. [アーキテクチャ](/nari-note-backend/Documents/architecture.md) - 各層の責務、エラーハンドリング

必要に応じて参照する:

- [認証・認可](/nari-note-backend/Documents/authentication-strategy.md) - 認証まわりを触るとき
- [ER 図](/nari-note-backend/Documents/er-diagram.md) - Entity / マイグレーションを触るとき
- [テストガイド](/nari-note-backend/Documents/testing-guide.md) - テストを書くとき
- [開発ワークフロー](/nari-note-backend/Documents/development-workflow.md) - ローカル実行、マイグレーション、設定キー

## 進め方

- 新しい API を追加するときは `new-api-endpoint` スキル（`.agents/skills/new-api-endpoint/SKILL.md`）の手順に従う
- API を追加・変更したら `python scripts/api-generator.py` でフロントの API コードを再生成する
- 実装前に類似の既存 Service / Controller を読み、パターンを揃える
- N+1 クエリ等、性能を明らかに落とす書き方をしない
- コメントは Why が必要な場合のみ書く
- 実装とドキュメントの不一致を見つけたら報告する
