# nari-note-engine

マイ定跡の研究盤が使う将棋エンジンのサービス。局面の解析と、2 局面が本質的に同じ形かの判定を行う。API サーバー（EC2）とは CPU を取り合わないよう別のマシンで動かし、バックエンドは `ShogiEngine:BaseUrl` に設定した URL を HTTP で呼ぶ。設定が無い環境では解析と判定が無効になる。

## 起動

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
| `ENGINE_CACHE` | 探索結果のキャッシュ（SQLite）。省略時は `cache.sqlite3` |
| `HOST` / `PORT` | 待ち受けアドレス。既定は `127.0.0.1:8790` |

## API

| パス | 入力 | 出力 |
| --- | --- | --- |
| `POST /analyze` | `{sfen}` | `{lines: [{usi, score, pv}]}`。score は手番側から見た値、MultiPV 3、200 万ノード |
| `POST /candidates` | `{sfen, others: [sfen]}` | `{candidates: [{sfen, unit_count}]}`。エンジンを使わずに、同じ形かもしれない局面を絞る |
| `POST /judge` | `{p, q}` | `{kind, delta, confirmed_depth, reason}` |
| `GET /health` | - | `{status}` |

`kind` は `same`（同じ）/ `same_with_delta`（Δ付きで同じ）/ `same_until`（浅い深さでは同じ）/ `different`（別）/ `skipped`（判定対象外）。

## 判定の考え方

盤面の見た目ではなく、この先の振る舞いが同じかで判定する。

1. 候補の絞り込み（`candidates.py`）: 手番が同じ、駒割の差が 1 枚以内、差分が 3 単位以内。差分の単位は、片方の局面にしかない駒を同じ種類同士で対応づけた 1 組
2. 最善手の一致（`judge.py`）: MultiPV 2、200 万ノード。1 位と 2 位の差が 30cp 未満のときは、相手の最善手を `searchmoves` で指させた評価値の低下が 30cp 未満なら一致とみなす
3. 差分が効いていないことの確認: 差分の一部だけを寄せた中間局面（最大 6 局面）で、最善手の後悔値が τ = max(50cp, 2 × 拮抗差) 未満
4. d 手先までの一致: d = 6 手。各手で 2 を繰り返し、評価値の差 Δ が δ = 50cp 以内なら「同じ」、Δ の幅が δ 以内なら「Δ付きで同じ」

探索は `Threads=1`、局面ごとに `usinewgame`、ノード数固定で決定的にしてキャッシュする。ノード上限で打ち切ると MultiPV の一部の行が前の反復の深さで出力されるため、`PvInterval=0` と `ConsiderationMode=true` で全反復を出させ、全行の深さがそろった最後の出力を採用する。
