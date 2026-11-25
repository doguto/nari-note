# nari-note-engine

マイ定跡の研究盤が使う将棋エンジン。局面の解析と、2 局面が本質的に同じ形かの判定を行う。本番は Lambda で動かし、API サーバー（EC2）は `ShogiEngine:FunctionName` の関数を Invoke する。ローカルでは同じ処理を HTTP サーバーで動かし、バックエンドは `ShogiEngine:BaseUrl` を呼ぶ。どちらも設定が無い環境では解析と判定が無効になる。

| ファイル | 内容 |
| --- | --- |
| `service.py` | 解析・候補の絞り込み・判定の入口。Lambda と HTTP サーバーで共通 |
| `lambda_function.py` | Lambda のハンドラー |
| `server.py` | ローカル用の HTTP サーバー |
| `judge.py` / `candidates.py` / `position.py` / `usi.py` | 判定、候補の絞り込み、局面の操作、USI エンジンとの通信 |
| `build-lambda.sh` | Lambda に配る zip を作る |

## ローカルで動かす

```bash
cd nari-note-engine
pip install -r requirements.txt
# やねうら王系のエンジン（評価関数込み）を engine/ に置くか、ENGINE_PATH で指定する
python server.py --port 8790
```

バックエンドは環境変数 `ShogiEngine__BaseUrl=http://localhost:8790` を付けて起動する。

| 環境変数 | 内容 |
| --- | --- |
| `ENGINE_PATH` | エンジンの実行ファイル。省略時は `engine/` の最初のファイル |
| `HOST` / `PORT` | 待ち受けアドレス。既定は `127.0.0.1:8790` |

## Lambda

関数は `<app_name>-shogi-engine`（Python 3.13、x86_64、1,769MB = 1 vCPU、タイムアウト 300 秒）。Terraform は `infrastruture/terraform/modules/app/engine`。

### ビルドとデプロイ

`build-lambda.sh` は Lambda と同じ Amazon Linux のイメージ（`public.ecr.aws/lambda/python:3.13`）の中で、やねうら王（NNUE、AVX2）を g++ でビルドし、評価関数・cshogi・このディレクトリの Python ファイルと合わせて `build/nari-note-engine.zip` にまとめる。Docker が必要。

| 環境変数 | 内容 |
| --- | --- |
| `EVAL_FILE` | NNUE の評価関数ファイル。既定は `engine/eval/nn.bin`。zip の中では `engine/eval/nn.bin` に置く |
| `YANEURAOU_REF` | ビルドするやねうら王のブランチまたはタグ。既定は `master` |

デプロイはリポジトリルートで `scripts/deploy-engine.sh` を実行する。zip をビルドして `<app_name>-deploy` バケットの `engine/releases/<日時>-<コミット>/` と `engine/latest/` に置き、Lambda のコードを差し替える。関数がまだ無い場合は zip を置くだけで終わるので、その後に `app/engine` を apply する（関数は `engine/latest/` の zip で作られる）。

### 呼び出し

ペイロードの `action` で処理を選ぶ。

| action | 入力 | 出力 |
| --- | --- | --- |
| `analyze` | `{sfen}` | `{lines: [{usi, score, pv}]}`。score は手番側から見た値、MultiPV 3、200 万ノード |
| `candidates` | `{sfen, others: [sfen]}` | `{candidates: [{sfen, unit_count}]}`。エンジンを使わずに、同じ形かもしれない局面を絞る |
| `judge` | `{p, q}` | `{kind, delta, confirmed_depth, reason}` |

HTTP サーバーでは同じ入出力を `POST /analyze` / `POST /candidates` / `POST /judge` で受ける（`GET /health` もある）。

`kind` は `same`（同じ）/ `same_with_delta`（Δ付きで同じ）/ `same_until`（浅い深さでは同じ）/ `different`（別）/ `skipped`（判定対象外）。

## 判定の考え方

盤面の見た目ではなく、この先の振る舞いが同じかで判定する。

1. 候補の絞り込み（`candidates.py`）: 手番が同じ、駒割の差が 1 枚以内、差分が 3 単位以内。差分の単位は、片方の局面にしかない駒を同じ種類同士で対応づけた 1 組
2. 最善手の一致（`judge.py`）: MultiPV 2、200 万ノード。1 位と 2 位の差が 30cp 未満のときは、相手の最善手を `searchmoves` で指させた評価値の低下が 30cp 未満なら一致とみなす
3. 差分が効いていないことの確認: 差分の一部だけを寄せた中間局面（最大 6 局面）で、最善手の後悔値が τ = max(50cp, 2 × 拮抗差) 未満
4. d 手先までの一致: d = 6 手。各手で 2 を繰り返し、評価値の差 Δ が δ = 50cp 以内なら「同じ」、Δ の幅が δ 以内なら「Δ付きで同じ」

探索は `Threads=1`、局面ごとに `usinewgame`、ノード数固定で決定的にする。1 回の判定の中では同じ探索を繰り返すので、リクエストの間だけ結果をメモリに持つ（リクエストをまたいでは持たない）。ノード上限で打ち切ると MultiPV の一部の行が前の反復の深さで出力されるため、`PvInterval=0` と `ConsiderationMode=true` で全反復を出させ、全行の深さがそろった最後の出力を採用する。
