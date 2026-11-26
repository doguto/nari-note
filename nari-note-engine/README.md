# nari-note-engine

マイ定跡の研究盤が使う将棋エンジン。局面の解析と、2 局面が本質的に同じ形かの判定を行う。本番は Lambda で動かし、API サーバー（EC2）は `ShogiEngine:FunctionName` の関数を Invoke する。ローカルでは同じ処理を HTTP サーバーで動かし、バックエンドは `ShogiEngine:BaseUrl` を呼ぶ。どちらも設定が無い環境では解析と判定が無効になる。

| ファイル | 内容 |
| --- | --- |
| `src/service.py` | 解析・候補の絞り込み・判定の入口。Lambda と HTTP サーバーで共通 |
| `src/lambda_function.py` | Lambda のハンドラー |
| `src/server.py` | ローカル用の HTTP サーバー |
| `src/judge.py` / `candidates.py` / `position.py` / `usi.py` | 判定、候補の絞り込み、局面の操作、USI エンジンとの通信 |
| `scripts/build-lambda.sh` | Lambda に配る zip を作る |
| `engine/` | エンジンと評価関数を `engine/<エンジン名>/` に置く（Git 管理外） |
| `build/` | ビルドした zip（Git 管理外） |

## エンジン

研究盤の解析では、次の 2 つからユーザーが選ぶ。どちらもやねうら王（GPLv3）に、やねうら王の作者が MIT License で公開している評価関数を載せたもの。評価関数は [pixivFANBOX](https://yaneurao.fanbox.cc/posts/12761272) から入手する。

| エンジン名 | 評価関数 | NNUE のアーキテクチャ |
| --- | --- | --- |
| `tsuyouraou-mini` | つようら王11mini | `SFNN_ka2_4096_8_64_progress8` |
| `tsuyouraou-micro` | つようら王11micro | `SFNN_ka2_2048_8_64_progress8` |

- アーキテクチャはやねうら王のビルド時に埋め込まれるので、評価関数ごとに実行ファイルを分ける
- progress 付きのアーキテクチャは、`nn.bin` と同じフォルダに `progress.bin` が要る。配布物に入っているライセンス表記も同じフォルダに置く
- `FV_SCALE` は作者が示す最適値の 24 にする（`src/service.py`）
- 同じ形の判定は `tsuyouraou-mini` に固定する（`JUDGE_ENGINE`）。解析で選ばれたエンジンは判定には使わない

```
engine/
  tsuyouraou-mini/
    YaneuraOu-by-gcc または *.exe   # ローカルで使う実行ファイル（zip にはビルドしたものを入れる）
    eval/nn.bin, eval/progress.bin
  tsuyouraou-micro/
    （同じ構成）
```

エンジン名は `src/service.py` の `ENGINES`、`scripts/build-lambda.sh` の `ENGINES`、バックエンドの `AnalysisEngine`、フロントの `features/book/utils/analysisEngine.ts` で揃える。

## ローカルで動かす

```bash
cd nari-note-engine
pip install -r requirements.txt
# engine/<エンジン名>/ に実行ファイルと評価関数を置くか、ENGINE_PATH で指定する
python src/server.py --port 8790
```

バックエンドは環境変数 `ShogiEngine__BaseUrl=http://localhost:8790` を付けて起動する。

| 環境変数 | 内容 |
| --- | --- |
| `ENGINE_PATH` | `engine/<エンジン名>/` に実行ファイルが無いときに、どのエンジン名でも使う実行ファイル。評価関数を埋め込んだエンジンで手元の動作だけ確かめるとき用 |
| `HOST` / `PORT` | 待ち受けアドレス。既定は `127.0.0.1:8790` |

## Lambda

関数は `<app_name>-shogi-engine`（Python 3.13、x86_64、1,769MB = 1 vCPU、タイムアウト 300 秒）。Terraform は `infrastruture/terraform/modules/app/engine`。

### タイムアウトと同時実行数

- 課金は実行時間に比例し、1 回の呼び出しは最長でタイムアウトの 300 秒分かかる。判定は通常数十秒で終わる
- バックエンドの Lambda クライアントは、タイムアウトを関数より長い 310 秒にし（Invoke API はクライアントの読み取りタイムアウトを関数のタイムアウトより長くするよう求めている）、自動再試行を切っている（`InfrastructureServiceInstaller`）。関数のタイムアウトを変えるときは、このクライアント側の値も合わせて変える
- 同時実行数は予約していない（`reserved_concurrency = null`）ので、リージョン内のアカウント全体の上限（既定 1,000）まで広がる。新規アカウントはこの上限が低く設定されていて、使用状況に応じて AWS が自動で引き上げる
- 予約できるのは「予約されていない同時実行数 − 100」まで（予約しない関数のために常に 100 が残される）。上限が低いうちは予約すると apply が失敗するので、上限が十分に上がってから、`prod/app/engine` の `inputs` で `reserved_concurrency` を設定して、同時に動く数（＝ 課金の上限）を抑える。上限は Service Quotas で引き上げを申請することもできる
- 上限に達すると Lambda は呼び出しを `TooManyRequestsException`（429）で断る。クライアントは再試行しないので、局面の解析はエラーになる。同じ形の判定は失敗にせず待ち行列に戻し、`ShapeJudgeWorker` の待機の後に判定し直す
- `ShapeJudgeProcessor` は判定を 1 件ずつ順に呼ぶので、判定が同時に使うのは 1 つだけ。判定を並列にするときは、並列度を同時実行数の上限（予約したならその値）より十分小さくし、解析の分を残す

### ビルドとデプロイ

`scripts/build-lambda.sh` は Lambda と同じ Amazon Linux のイメージ（`public.ecr.aws/lambda/python:3.13`）の中で、エンジンごとにやねうら王（AVX2）を g++ でビルドし、`engine/<エンジン名>/eval/`・cshogi・`src/` の Python ファイル（`server.py` を除く）と合わせて `build/nari-note-engine.zip` にまとめる。Docker が必要。

| 環境変数 | 内容 |
| --- | --- |
| `YANEURAOU_REF` | ビルドするやねうら王のコミット SHA（ブランチ名・タグ名も可）。既定は `scripts/build-lambda.sh` に固定した SHA |

判定はエンジンの探索結果が変わらないことを前提にしているので、やねうら王の版は `scripts/build-lambda.sh` の既定値で固定している。版を上げると評価値や最善手が変わり、上げる前と後の判定結果が混ざる。上げるときは既定値の SHA を書き換えてコミットし、デプロイする。

デプロイはリポジトリルートで `scripts/deploy-engine.sh` を実行する。zip をビルドして `<app_name>-deploy` バケットの `engine/releases/<日時>-<コミット>/` と `engine/latest/` に置き、Lambda のコードを差し替える。関数がまだ無い場合は zip を置くだけで終わるので、その後に `app/engine` を apply する（関数は `engine/latest/` の zip で作られる）。

### 呼び出し

ペイロードの `action` で処理を選ぶ。

| action | 入力 | 出力 |
| --- | --- | --- |
| `analyze` | `{sfen, engine}` | `{lines: [{usi, score, pv}]}`。score は手番側から見た値、MultiPV 3、200 万ノード |
| `candidates` | `{sfen, others: [sfen]}` | `{candidates: [{sfen, unit_count}]}`。エンジンを使わずに、同じ形かもしれない局面を絞る |
| `judge` | `{p, q}` | `{kind, delta, confirmed_depth, reason}` |

HTTP サーバーでは同じ入出力を `POST /analyze` / `POST /candidates` / `POST /judge` で受ける（`GET /health` もある）。

`kind` は `same`（同じ）/ `same_with_delta`（Δ付きで同じ）/ `same_until`（浅い深さでは同じ）/ `different`（別）/ `skipped`（判定対象外）。

## 判定の考え方

盤面の見た目ではなく、この先の振る舞いが同じかで判定する。

1. 候補の絞り込み（`src/candidates.py`）: 手番が同じ、駒割の差が 1 枚以内、差分が 3 単位以内。差分の単位は、片方の局面にしかない駒を同じ種類同士で対応づけた 1 組
2. 最善手の一致（`src/judge.py`）: MultiPV 2、200 万ノード。1 位と 2 位の差が 30cp 未満のときは、相手の最善手を `searchmoves` で指させた評価値の低下が 30cp 未満なら一致とみなす
3. 差分が効いていないことの確認: 差分の一部だけを寄せた中間局面（最大 6 局面）で、最善手の後悔値が τ = max(50cp, 2 × 拮抗差) 未満
4. d 手先までの一致: d = 6 手。各手で 2 を繰り返し、評価値の差 Δ が δ = 50cp 以内なら「同じ」、Δ の幅が δ 以内なら「Δ付きで同じ」

探索は `Threads=1`、局面ごとに `usinewgame`、ノード数固定で決定的にする。1 回の判定の中では同じ探索を繰り返すので、リクエストの間だけ結果をメモリに持つ（リクエストをまたいでは持たない）。ノード上限で打ち切ると MultiPV の一部の行が前の反復の深さで出力されるため、`PvInterval=0` と `ConsiderationMode=true` で全反復を出させ、全行の深さがそろった最後の出力を採用する。

## 参考

- [Lambda のクォータ](https://docs.aws.amazon.com/ja_jp/lambda/latest/dg/gettingstarted-limits.html)
- [予約済み同時実行数の設定](https://docs.aws.amazon.com/ja_jp/lambda/latest/dg/configuration-concurrency.html)
- [Invoke API（TooManyRequestsException、クライアントのタイムアウト）](https://docs.aws.amazon.com/lambda/latest/api/API_Invoke.html)
- [Lambda での AVX2 の利用](https://docs.aws.amazon.com/lambda/latest/dg/runtimes-avx2.html)
