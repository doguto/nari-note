# .agents ディレクトリ

AI エージェント（GitHub Copilot、Claude Code 等）が使うスキルの実体を一元管理するディレクトリ。

## 構成

```
.agents/
└── skills/
    └── <skill-name>/
        └── SKILL.md
```

各エージェント用のディレクトリからは、`skills` ディレクトリ全体をシンボリックリンクで参照している。

| リンク | 参照先 |
| --- | --- |
| `.github/skills` | `../.agents/skills` |
| `.claude/skills` | `../.agents/skills` |

そのため `.agents/skills/` 配下にスキルを追加・編集するだけで、すべてのエージェントに反映される。個別のリンクを作る必要はない。

Windows で clone する場合、シンボリックリンクを有効にするには `git config core.symlinks true`（と、開発者モードまたは管理者権限）が必要。

## 管理しているスキル

スキルの一覧は `.agents/skills/` 配下のディレクトリを参照する。各スキルの用途は `SKILL.md` の front matter（`name` / `description`）に書く。

## スキルの追加方法

```bash
mkdir .agents/skills/<skill-name>
# .agents/skills/<skill-name>/SKILL.md を作成し、front matter に name と description を書く
```
