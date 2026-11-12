---
name: Agent-Doc-Updater-Skill
description: |-
    各エージェントのドキュメント(.agent.md)を最新のプロジェクト状態に照らして検証し、必要に応じて手で更新します
    以下の場合に使用する
    - プロジェクト構造が大きく変更された後
    - ドキュメントの追加・削除・移動の後
    - エージェントの理解を最新の状態に保ちたい場合
---

# Agent Doc Updater Skill

## 概要

`.github/agents/` 配下の各エージェント定義（`backend-engineer.agent.md`, `frontend-engineer.agent.md`, `spec.agent.md`）が、現在のリポジトリと食い違っていないかを確認し、食い違いがあれば手で直すスキル。

`scripts/update-agent-docs.py` は**検証のみ**を行い、ファイルは書き換えない。`--dry-run` を付けても付けなくても動作は同じ（表示メッセージが変わるだけ）。

スクリプトが検証する内容:

- 各 `.agent.md` 内の Markdown リンクのリンク切れ（`/` 始まりはリポジトリルートからのパスとして解決）
- backend / frontend エージェントについて、スクリプト内にハードコードされた「想定ドキュメント一覧」が `nari-note-backend/Documents/` / `nari-note-frontend/docs/` に存在するか

## 実行手順

1. リポジトリルートで検証スクリプトを実行する

```bash
python scripts/update-agent-docs.py --verbose
```

- `--agent backend-engineer|frontend-engineer|spec`: 対象を 1 つに絞る
- `--verbose`: 分析の詳細ログを表示する

2. 出力された警告を確認する
   - リンク切れ: 該当 `.agent.md` のリンクを実在するパスに直す
   - 想定ドキュメントの欠落: ドキュメントの統合・削除が原因で、`.agent.md` 側が正しい場合は警告を無視してよい（想定一覧はスクリプトに直書きされており、ドキュメント構成の変更に追従していないことがある）

3. 警告以外の内容も目視で確認し、`.agent.md` を手で更新する
   - 技術スタックや規約は `.agent.md` に重複して書かず、ルートの `AGENTS.md` を参照させる
   - 参照ドキュメントの一覧が、AGENTS.md の「ドキュメント地図」と一致しているか
