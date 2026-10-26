---
description: PRをレビューしバグやCLAUDE.md違反を指摘する（単一エージェントで完結、サブエージェントは使わない）
argument-hint: [owner/repo/pull/number] [--comment]
allowed-tools: Bash(gh pr view:*), Bash(gh pr diff:*), Bash(gh pr comment:*), Bash(gh api:*), Read, Glob, Grep
---

$ARGUMENTS で指定された Pull Request をレビューする。

**重要な設計方針:** このコマンドは `Task`（サブエージェント起動）を一切使わない。理由: ヘッドレスCI実行（`claude-code-action`）では `allowed-tools` は「自動承認リスト」に過ぎず、リストに無いツールを呼ぶと人間の確認が取れず自動的に拒否される（`permission_denials_count` が増える）。`Task` は非対話環境で信頼できないため、このコマンド自身の中で完結するステップだけを使う。

## 手順

1. **事前チェック**
   `gh pr view <PR> --json state,isDraft,title,body,comments,headRefOid` で以下を確認し、該当すれば即座に終了する（以降の処理をしない）。
   - PR が closed である
   - PR が draft である
   - 明らかにレビュー不要（自動生成PR、typo修正など自明に問題ない変更）
   - 取得した `headRefOid` に対して、既に `<!-- claude-review-sha: <headRefOid> -->` を含む claude (bot) のコメントが存在する（＝この commit は既にレビュー済み）
     - **注意:** 「claude (bot) がコメント済みかどうか」だけで判定しないこと。新しい commit が push されると `headRefOid` が変わるため、過去のレビュー時のマーカーとは一致せず、正しく再レビューされる

2. **差分の把握**
   `gh pr diff <PR>` で差分を取得する。必要に応じて `Read` / `Grep` / `Glob` で差分周辺のファイルを読み、文脈を補う（変更されていない既存コードとの整合性確認のため）。
   ルートの CLAUDE.md（`@AGENTS.md` 経由で AGENTS.md を含む）はセッション開始時に自動で読み込まれているので、追加で読む必要はない。
   差分が `nari-note-backend/` に及ぶ場合は `nari-note-backend/Documents/backend-implementation-guide.md`、`nari-note-frontend/` に及ぶ場合は `nari-note-frontend/docs/implementation-guide.md` も規約として参照してよい。

3. **レビュー観点**
   以下の **HIGH SIGNAL** な問題だけを拾う。誤検知はレビューの信頼を損なうので、確信が持てないものは指摘しない。

   - コンパイル/パースが確実に失敗するコード（構文エラー、型エラー、未解決の参照）
   - 入力に関わらず確実に誤った結果を生む明確なロジックエラー
   - CLAUDE.md の明確なルール違反（該当ルールを引用できるもの。ルールが適用されるファイルパス・スコープであることも確認する）

   以下は **指摘しない**:
   - コードスタイル・品質に関する主観的な指摘
   - 特定の入力や状態に依存する潜在的な問題
   - 些細な指摘（シニアエンジニアなら流すレベル）
   - リンターが検出するような問題（リンター自体は実行しない）
   - テストカバレッジ不足など、CLAUDE.mdで明示的に要求されていない一般的な品質懸念
   - すでに存在していた問題（今回の差分で生まれたものではない）
   - コード中で明示的に抑制されている問題（lintの抑制コメントがある等）

   **教育的コメント（任意）**
   上記のバグ指摘とは別枠で、次の3カテゴリそれぞれ最大1件まで「学びのポイント」を添えてよい。これは修正を求める指摘ではなく気づきの共有なので、後述のフォーマットで明確に区別すること。断定調ではなく説明調で書き（「〜が誤り」ではなく「〜という理由でこの書き方になっていると思われる」）、確信が持てない推測は書かない。該当箇所が無いカテゴリは無理に書かない。

   - **ソフトウェア設計的観点**: レイヤー責務の分離やアーキテクチャ、切り出し過ぎ・過小な抽象化の判断など
   - **パフォーマンス的観点**: EF Core の N+1 クエリや不要な全件取得、TanStack Query のキャッシュ・再フェッチ設計、不要な再レンダリングなど
   - **その他**: 上記に当てはまらないが、学びになる点

4. **出力**
   まずレビュー結果をターミナルに要約する。バグ指摘と教育的コメントは別物として扱う（教育的コメントが何件あっても「問題あり」判定にはしない）。
   - バグ・CLAUDE.md違反が見つかった場合は各問題を簡潔に説明する
   - バグ・CLAUDE.md違反が見つからなかった場合は「No issues found. Checked for bugs and CLAUDE.md compliance.」と出力する
   - 教育的コメントがあれば、バグ指摘とは別枠でその内容も要約する

   `--comment` 引数が **無い** 場合はここで終了する。GitHubへのコメント投稿は行わない。

   `--comment` 引数が **ある** 場合:
   - バグがあれば、各問題ごとに `gh api repos/<owner>/<repo>/pulls/<number>/comments` (POST) でインラインコメントを投稿する（詳細は後述）。教育的コメントがあればこれもインラインで投稿する。
   - **最後に、バグの有無にかかわらず必ず** `gh pr comment <PR> --body "..."` でサマリコメントを1件投稿する。このコメント冒頭の `<!-- claude-review-sha: ... -->` が「この commit はレビュー済み」の目印になり、次回 synchronize 時の事前チェック（手順1）がこれを見て再レビューの要否を判定する。**このマーカーを省略すると次回以降レビューが二度と走らなくなるので、必ず含めること。**

     バグが無かった場合:
     ```
     <!-- claude-review-sha: <headRefOid> -->
     ## Code review

     No issues found. Checked for bugs and CLAUDE.md compliance.
     ```

     バグがあった場合:
     ```
     <!-- claude-review-sha: <headRefOid> -->
     ## Code review

     N件の問題を上記のインラインコメントで指摘しました。
     ```

     各問題のインラインコメントは以下のコマンドで投稿する:
     ```
     gh api repos/<owner>/<repo>/pulls/<number>/comments \
       -f body="<コメント本文>" \
       -f commit_id="<PRのhead commit SHA>" \
       -f path="<ファイルパス>" \
       -F line=<行番号> \
       -f side="RIGHT"
     ```
     - `commit_id` は `gh pr view <PR> --json headRefOid -q .headRefOid` で取得したPRのhead commitのSHAを使うこと（ベースブランチのSHAではない）
     - 問題の説明を簡潔に書く
     - 小さく自己完結した修正であれば committable suggestion を含める。修正がその提案だけで完全に完結する場合のみ使うこと（追加対応が必要な場合は提案ブロックを付けない）
     - 1つの問題につき1コメントのみ。重複投稿はしない
     - CLAUDE.md 違反を指摘する場合は該当ファイルへのリンクを含める
     - `gh api` がエラーを返した場合（行が diff の範囲外など）はその行へのインライン投稿を諦め、`gh pr comment` で通常コメントとして投稿する（無理にリトライしない）

   - 教育的コメントがあれば、バグ指摘と同じ `gh api` の手順でインラインコメントを投稿する。ただしバグ指摘との混同を避けるため、本文の先頭に必ずカテゴリラベルを付ける。
     ```
     **[学びのポイント / ソフトウェア設計]** <説明>
     **[学びのポイント / パフォーマンス]** <説明>
     **[学びのポイント / その他]** <説明>
     ```
     - committable suggestion は付けない（修正提案ではなく気づきの共有のため）
     - バグ指摘用コメントと教育的コメントは同じ行に重複投稿してよいが、1カテゴリにつき1コメントまで

## 注意事項

- GitHubとのやり取りは `gh` CLI を使うこと。WebFetch は使わない
- コード中のリンク（例: 該当箇所への参照）を貼る場合は、必ずフルの git commit SHA を使うこと。`gh pr view <PR> --json headRefOid -q .headRefOid` で取得できる。省略形やコマンド埋め込みでは Markdown プレビューが正しく描画されない
