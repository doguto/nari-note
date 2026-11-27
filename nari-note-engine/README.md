# nari-note-engine

マイ定跡の研究盤が使う将棋エンジン。局面を解析し、候補手と評価値を返す。本番は Lambda で動かし、API サーバー（EC2）は `ShogiEngine:FunctionName` の関数を Invoke する。ローカルでは同じ処理を HTTP サーバーで動かし、バックエンドは `ShogiEngine:BaseUrl` を呼ぶ。どちらも設定が無い環境では解析が無効になる。

| ファイル | 内容 |
| --- | --- |
| `src/service.py` | 解析の入口。Lambda と HTTP サーバーで共通 |
| `src/lambda_function.py` | Lambda のハンドラー |
| `src/server.py` | ローカル用の HTTP サーバー |
| `src/position.py` / `usi.py` | SFEN の正規化、USI エンジンとの通信 |
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

```
engine/
  tsuyouraou-mini/
    YaneuraOu-by-gcc または *.exe   # ローカルで使う実行ファイル（zip にはビルドしたものを入れる）
    eval/nn.bin, eval/progress.bin
  tsuyouraou-micro/
    （同じ構成）
```

エンジン名は `src/service.py` の `ENGINES`、`scripts/build-lambda.sh` の `ENGINES`、バックエンドの `AnalysisEngine`、フロントの `features/playbook/utils/analysisEngine.ts` で揃える。

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

- 課金は実行時間に比例し、1 回の呼び出しは最長でタイムアウトの 300 秒分かかる。解析は通常数秒で終わる
- バックエンドの Lambda クライアントは、タイムアウトを関数より長い 310 秒にし（Invoke API はクライアントの読み取りタイムアウトを関数のタイムアウトより長くするよう求めている）、自動再試行を切っている（`InfrastructureServiceInstaller`）。関数のタイムアウトを変えるときは、このクライアント側の値も合わせて変える
- 同時実行数は予約していない（`reserved_concurrency = null`）ので、リージョン内のアカウント全体の上限（既定 1,000）まで広がる。新規アカウントはこの上限が低く設定されていて、使用状況に応じて AWS が自動で引き上げる
- 予約できるのは「予約されていない同時実行数 − 100」まで（予約しない関数のために常に 100 が残される）。上限が低いうちは予約すると apply が失敗するので、上限が十分に上がってから、`prod/app/engine` の `inputs` で `reserved_concurrency` を設定して、同時に動く数（＝ 課金の上限）を抑える。上限は Service Quotas で引き上げを申請することもできる
- 上限に達すると Lambda は呼び出しを `TooManyRequestsException`（429）で断る。クライアントは再試行しないので、局面の解析はエラーになる

### ビルドとデプロイ

`scripts/build-lambda.sh` は Lambda と同じ Amazon Linux のイメージ（`public.ecr.aws/lambda/python:3.13`）の中で、エンジンごとにやねうら王（AVX2）を g++ でビルドし、`engine/<エンジン名>/eval/`・cshogi・`src/` の Python ファイル（`server.py` を除く）と合わせて `build/nari-note-engine.zip` にまとめる。Docker が必要。

| 環境変数 | 内容 |
| --- | --- |
| `YANEURAOU_REF` | ビルドするやねうら王のコミット SHA（ブランチ名・タグ名も可）。既定は `scripts/build-lambda.sh` に固定した SHA |

ビルドする日によって評価値や最善手が変わらないよう、やねうら王の版は `scripts/build-lambda.sh` の既定値で固定している。上げるときは既定値の SHA を書き換えてコミットし、デプロイする。

デプロイはリポジトリルートで `scripts/deploy-engine.sh` を実行する。zip をビルドして `<app_name>-deploy` バケットの `engine/releases/<日時>-<コミット>/` と `engine/latest/` に置き、Lambda のコードを差し替える。関数がまだ無い場合は zip を置くだけで終わるので、その後に `app/engine` を apply する（関数は `engine/latest/` の zip で作られる）。

### 呼び出し

ペイロードの `action` で処理を選ぶ（今は `analyze` だけ）。

| action | 入力 | 出力 |
| --- | --- | --- |
| `analyze` | `{sfen, engine}` | `{lines: [{usi, score, pv}]}`。score は手番側から見た値、MultiPV 3、200 万ノード |

HTTP サーバーでは同じ入出力を `POST /analyze` で受ける（`GET /health` もある）。

## 探索

探索は `Threads=1`、局面ごとに `usinewgame`、ノード数固定で決定的にする。ノード上限で打ち切ると MultiPV の一部の行が前の反復の深さで出力されるため、`PvInterval=0` と `ConsiderationMode=true` で全反復を出させ、全行の深さがそろった最後の出力を採用する。

## 参考

- [Lambda のクォータ](https://docs.aws.amazon.com/ja_jp/lambda/latest/dg/gettingstarted-limits.html)
- [予約済み同時実行数の設定](https://docs.aws.amazon.com/ja_jp/lambda/latest/dg/configuration-concurrency.html)
- [Invoke API（TooManyRequestsException、クライアントのタイムアウト）](https://docs.aws.amazon.com/lambda/latest/api/API_Invoke.html)
- [Lambda での AVX2 の利用](https://docs.aws.amazon.com/lambda/latest/dg/runtimes-avx2.html)
