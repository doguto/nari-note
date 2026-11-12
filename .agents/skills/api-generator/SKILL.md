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

`scripts/api-generator.py` はバックエンドの C# コードを正規表現で解析し、フロントエンドの API コードを生成する。

入力:

- `nari-note-backend/Src/Controller/*Controller.cs`（`[HttpGet]` 等のアクション）
- `nari-note-backend/Src/Application/Dto/`（`Request/`, `Response/` と直下の DTO）
- `nari-note-backend/Src/Domain/ValueObject/EntityKeyObject.cs`（ID 型）

出力（`nari-note-frontend/src/lib/api/`、毎回上書き）:

| ファイル | 内容 |
| --- | --- |
| `types.ts` | Request / Response / DTO の型 |
| `endpoints.ts` | Axios クライアントを使う API 関数（ブラウザから BFF 経由で呼ぶ） |
| `hooks.ts` | TanStack Query のフック（`useXxx`） |
| `server.ts` | Server Component 用の fetch 関数（GET エンドポイントのみ） |

`client.ts` と `index.ts` は生成対象外（手書き）。

## 実行手順

1. リポジトリルートで実行する（パスはルートからの相対で解決される）

```bash
PYTHONIOENCODING=utf-8 python scripts/api-generator.py --verbose
```

- `--verbose`: レスポンス型を特定できずスキップしたアクションの件数も表示する
- `--force`: 互換のために残っているオプション。指定しなくても常に全ファイルを上書きする
- Windows のコンソールでは出力の文字化け・エンコードエラーを避けるため `PYTHONIOENCODING=utf-8` を付ける

2. 出力の末尾に `Skipped methods` が出ていないか確認する。レスポンス型は次のいずれかで決まるので、スキップされた場合はどちらかに合わせる
   - アクションの戻り値を `ActionResult<XxxResponse>` にする
   - アクション名 `Xxx` に対して `XxxResponse` という Response DTO を用意する（どちらも無い DELETE と Logout は戻り値なしとして扱われる）

3. `git diff nari-note-frontend/src/lib/api/` で生成結果を確認する。生成物は手で編集しない（次回の生成で上書きされる）
