---
name: Api-Generator-Skill
description: |-
    バックエンドで実装したApiに基づき、フロントエンドのAPI関連コードを自動生成します
    以下で使用する
    - 新規APIの実装完了後
    - 既存APIのRequest、Responseの更新後
---

# Api Generator Skill

## 概要

作成・更新したAPI情報をフロントエンドのコードに反映させるスクリプトを実行する

## 実行手順

1. Pythonスクリプトの実行

リポジトリ直下の `scripts` ディレクトリに存在するPythonスクリプトを実行することでフロントエンドのコードが自動で更新される

```bash
python scripts/api-generator.py --force --verbose
```

2. 生成されたコードの確認

スクリプトの実行のみで生成が完了するので、念のため以下のファイル内容を確認し変更内容を把握する

- nari-note-frontend/src/lib/api/endpoint.ts
- nari-note-frontend/src/lib/api/hooks.ts
- nari-note-frontend/src/lib/api/types.ts
