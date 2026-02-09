---
name: Agent-Doc-Updater-Skill
description: |-
    各エージェントのドキュメント(.agent.md)を最新のプロジェクト状態に基づいて更新します
    以下の場合に使用する
    - プロジェクト構造が大きく変更された後
    - 新しい重要なドキュメントが追加された後
    - エージェントの理解を最新の状態に保ちたい場合
---

# Agent Doc Updater Skill

## 概要

各エージェント（backend-engineer, frontend-engineer, spec）の`.agent.md`ファイルを、
最新のプロジェクト構造とドキュメント情報に基づいて更新するスキルです。

## 実行手順

1. Pythonスクリプトの実行

リポジトリ直下の `scripts` ディレクトリに存在するPythonスクリプトを実行することで
各エージェントのドキュメントが自動で更新される

```bash
python scripts/update-agent-docs.py --verbose
```

### オプション

- `--agent`: 特定のエージェントのみ更新（backend-engineer, frontend-engineer, spec）
- `--dry-run`: 実際には更新せず、変更内容のプレビューのみ表示
- `--verbose`: 詳細な実行ログを表示

### 使用例

```bash
# 全エージェントのドキュメントを更新
python scripts/update-agent-docs.py --verbose

# バックエンドエージェントのみ更新
python scripts/update-agent-docs.py --agent backend-engineer --verbose

# ドライランで変更内容を確認
python scripts/update-agent-docs.py --dry-run --verbose
```

2. 更新されたドキュメントの確認

スクリプト実行後、以下のファイルが更新されているか確認する

- `.github/agents/backend-engineer.agent.md`
- `.github/agents/frontend-engineer.agent.md`
- `.github/agents/spec.agent.md`

## 更新内容

このスキルは以下の情報を各エージェントのドキュメントに反映します：

### Backend-Engineer-Agent

- バックエンドディレクトリ配下の構造
- 主要なドキュメントファイルの存在確認とリンク
- 技術スタックの最新情報

### Frontend-Engineer-Agent

- フロントエンドディレクトリ配下の構造
- 主要なドキュメントファイルの存在確認とリンク
- 技術スタックの最新情報

### Spec-Agent

- 仕様書ディレクトリ（spec/）の構造
- 利用可能な仕様書ファイルのリスト
