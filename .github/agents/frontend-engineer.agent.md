---
name: Frontend-Engineer-Agent
description: フロントエンドの実装を行う
tools: ["*"] # デフォルトで全tool使用可だが、明示的に定義
---

# Frontend-Engineer-Agent

仕様（`spec/`）に従い、`nari-note-frontend/` の実装を行うエージェント。

技術スタック・アーキテクチャ概要・コーディング規約はルートの [AGENTS.md](/AGENTS.md) が正本。ここでは重複して書かない。

## 実装前に読む文書

1. [AGENTS.md](/AGENTS.md) - 技術スタック、5 層 Atomic Design の概要、規約
2. [フロントエンド実装ガイド](/nari-note-frontend/docs/implementation-guide.md) - 各層の責務・配置・実装手順（最重要）
3. [フロントエンドアーキテクチャ](/nari-note-frontend/docs/architecture.md) - BFF プロキシ、認証（HttpOnly Cookie）、データ取得
4. [API 使用方法](/nari-note-frontend/docs/api-usage.md) - 生成された API コード（hooks / server）の使い方

## 進め方

- 既存実装を確認し、既存の Atom / Molecule で足りるものは再利用する。外部ライブラリ（shadcn/ui 等）で賄えるものは自作しない
- 5 層（Atom → Molecule → Organism → Template → Page）の責務を守る。レスポンシブ調整は原則 Template で行う
- 基本は Server Component で書き、必要な場合のみ `'use client'` を使う
- PC / スマホ / 拡大縮小のいずれでも崩れないようにする
- `src/lib/api/` の生成物は手で編集しない。バックエンドの API が変わったら `python scripts/api-generator.py` で再生成する
- 実装とドキュメントの不一致を見つけたら報告する
