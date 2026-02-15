# .agent ディレクトリ

このディレクトリはAIエージェント関連のリソースを一元管理するための親ディレクトリです。

## ディレクトリ構造

```
.agent/
└── skills/
    ├── agent-doc-updater/
    │   └── SKILL.md
    └── api-generator/
        └── SKILL.md
```

## Skills

`skills/` ディレクトリには、各種AIエージェントが使用するスキル定義ファイルが格納されています。

### 管理方針

- 実際のSKILL.mdファイルは `.agent/skills/` 配下に配置
- 他のAIエージェントディレクトリ（`.github/skills/`, `.claude/skills/`）からはシンボリックリンクで参照
- skillの内容を更新する場合は、`.agent/skills/` 配下のファイルを編集することで、すべてのAIエージェントに変更が反映される

### 既存のシンボリックリンク

#### .github/skills/
- `agent-doc-updater` → `../../.agent/skills/agent-doc-updater`
- `api-generator` → `../../.agent/skills/api-generator`

#### .claude/skills/
- `agent-doc-updater` → `../../.agent/skills/agent-doc-updater`

## 新しいスキルの追加方法

1. `.agent/skills/` 配下に新しいディレクトリを作成
2. SKILL.mdファイルを作成
3. 必要に応じて `.github/skills/` や `.claude/skills/` からシンボリックリンクを作成

```bash
# 例: 新しいスキル "new-skill" を追加する場合
mkdir -p .agent/skills/new-skill
touch .agent/skills/new-skill/SKILL.md

# .github/skills/ からリンク
ln -s ../../.agent/skills/new-skill .github/skills/new-skill

# .claude/skills/ からリンク
ln -s ../../.agent/skills/new-skill .claude/skills/new-skill
```
