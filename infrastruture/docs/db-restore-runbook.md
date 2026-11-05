# DB リストア手順書 (Runbook)

障害時に DB を復旧するための手順。仕組み・設計の背景は [db-backup-recovery.md](db-backup-recovery.md) を参照。

> [!WARNING]
> `restore` / `extract` は本番で初めて使うことがないよう、事前に dev 環境で訓練しておくこと ([訓練](#訓練))。

## 0. 共通の準備

### EC2 へ接続する

```bash
# SSH
ssh -i ~/.ssh/nari-note ec2-user@<EIP>

# SSM Session Manager (SSH を使わない場合)
aws-vault exec narinote -- aws ssm start-session --target <instance-id> --region ap-northeast-1
```

EIP・インスタンス ID は `terragrunt output` (`prod/app/server`) で確認できる。

### 現状を確認する

```bash
sudo systemctl status postgresql --no-pager     # PostgreSQL が動いているか
sudo nari-note-deploy status                   # アプリの稼働系統
sudo nari-note-db-backup list                  # S3 上のベースバックアップ一覧 (Finish time を確認)
mountpoint /data/postgresql                    # データ用 EBS がマウントされているか
```

### コマンド一覧

| コマンド | 内容 |
| --- | --- |
| `sudo nari-note-db-backup list` | S3 上のベースバックアップ一覧 |
| `sudo nari-note-db-backup restore <latest\|時刻>` | アプリ・PG を停止し、S3 から復元する。完了後もアプリは停止したまま |
| `sudo nari-note-db-backup finalize` | restore を確定する。退避データ削除 → ベースバックアップ取得 → アプリ起動 |
| `sudo nari-note-db-backup extract <時刻>` | 本番を止めずに、指定時刻の DB を port 5433 で一時起動する |
| `sudo nari-note-db-backup extract-stop` | 一時 DB を停止・削除する |
| `sudo nari-note-db-backup base` | ベースバックアップを手動で取得する |

- 時刻は**オフセット付き**で指定する (例: `'2026-11-05 14:31:00+09:00'`)。オフセットが無いとエラーになる
- `restore` / `finalize` は確認プロンプトが出る。`-y` を付けるとスキップする

## 1. どの手順を使うか

| 状況 | 手順 | 失うデータ | 停止時間の目安 |
| --- | --- | --- | --- |
| PostgreSQL / EC2 が落ちただけ (EBS は無事) | [A](#a-postgresql--ec2-の障害-ebs-は無事) | なし | 数分 |
| 誤操作・壊れたマイグレーションで DB 全体を戻したい | [B](#b-db-全体を時刻-t-へ戻す) | 時刻 T 以降 (意図的に破棄) | 10〜20 分 |
| 一部の行・テーブルだけ戻したい | [C](#c-一部の行テーブルだけ戻す-本番無停止) | なし | 停止なし |
| データ用 EBS が壊れた・消えた | [D](#d-データ用-ebs-の喪失) | 最大約 1 分 | 20〜30 分 |
| AZ ごと使えない | [E](#e-az-障害) | 最大約 1 分 | 30 分〜 |
| S3 のバックアップが使えない (WAL 欠損・バケット喪失) | [F](#f-ebs-スナップショットから復元-最終手段) | 最大 24 時間 | 20〜30 分 |

## A. PostgreSQL / EC2 の障害 (EBS は無事)

データは EBS に残っているため、起動し直すだけでよい。

**PostgreSQL だけが落ちた場合** (EC2 上で)

```bash
sudo systemctl restart postgresql
sudo journalctl -u postgresql -n 50 --no-pager   # 起動時のクラッシュリカバリが完了したか確認
sudo nari-note-deploy status
```

**EC2 ごと壊れた場合** (手元で)

```bash
cd infrastruture/terraform/prod/app/server
aws-vault exec narinote -- terragrunt apply -replace=aws_instance.app_server
```

userdata が既存の EBS をマウントし、PostgreSQL・アプリを起動する。EIP は付け直されるため Cloudflare の変更は不要。

## B. DB 全体を時刻 T へ戻す

### B-1. 時刻 T を決める

CloudWatch Logs Insights で問題の操作・マイグレーションの時刻を特定し、**その直前**を T とする。

- アプリログ: `/nari-note/application`
- アクセスログ: `/nari-note/nginx/access`

```bash
sudo nari-note-db-backup list   # T より前に Finish time のベースバックアップがあることを確認
```

### B-2. (原因がマイグレーションの場合のみ) アプリを旧バージョンへ戻す

アプリは起動時にマイグレーションを適用するため、先に戻しておかないと同じマイグレーションが再適用される。

```bash
sudo nari-note-deploy rollback
```

### B-3. 復元する

```bash
sudo nari-note-db-backup restore '2026-11-05 14:31:00+09:00'
```

アプリ停止 → PostgreSQL 停止 → ベースバックアップ取得 → 現データを `/data/postgresql/.pre-restore-<時刻>` へ退避 → WAL を T まで再生、まで自動で行う。**ここからダウンタイム**。

### B-4. データを確認する

```bash
sudo -u postgres psql -d nari_note
```

### B-5. 確定する

```bash
sudo nari-note-db-backup finalize
```

退避データを削除し、新しいベースバックアップを取ってからアプリを起動する。**ここでダウンタイム終了**。

> [!IMPORTANT]
> finalize を実行するまで退避データは残る。確認で問題が見つかった場合は finalize せず、[restore を取り消す](#restore-を取り消して元のデータに戻す) か、別の時刻で restore し直す (前回分の退避データが残っていると restore はエラーになるため、先に取り消すこと)。

## C. 一部の行・テーブルだけ戻す (本番無停止)

```bash
# 1. 時刻 T の DB を port 5433 で一時起動する (B-1 と同様に T を決める)
sudo nari-note-db-backup extract '2026-11-05 14:31:00+09:00'

# 2. 必要なデータを抽出する
sudo -u postgres psql -p 5433 -d nari_note        # 対象の行を確認
# テーブル名は EF Core の命名 (大文字を含む) のため、ダブルクォートで囲む
sudo -u postgres pg_dump -p 5433 -d nari_note -t '"<テーブル名>"' --data-only > /tmp/rows.sql

# 3. 抽出結果から必要な行だけを本番 (5432) に投入する
sudo -u postgres psql -p 5432 -d nari_note

# 4. 片付ける
sudo nari-note-db-backup extract-stop
```

- 一時 DB はルートボリューム (8GiB) の `/var/tmp/nari-note-pg-extract` に展開される。DB が大きくなった場合は空き容量に注意する
- 本番への投入は主キーの重複に注意し、必要な行だけを `INSERT` する

## D. データ用 EBS の喪失

### D-1. 新しい EBS で EC2 を作り直す (手元で)

```bash
cd infrastruture/terraform/prod/app/server
# prevent_destroy のため、壊れた EBS は削除せず Terraform の管理から外す
aws-vault exec narinote -- terragrunt state rm aws_ebs_volume.postgres_data
aws-vault exec narinote -- terragrunt apply
```

- Terraform が新しい空の EBS を作成し、ボリューム ID の変更で userdata が変わるため EC2 も再作成される
- 新しい EC2 は空の EBS を初期化し、**空の DB でアプリが起動する** (次の手順ですぐ止める)

### D-2. S3 の最新状態まで復元する (EC2 上で)

```bash
sudo nari-note-db-backup restore latest
sudo -u postgres psql -d nari_note     # データを確認
sudo nari-note-db-backup finalize
```

### D-3. 後片付け

管理から外した古い EBS はコンソールから削除する (DLM のスナップショット対象タグが付いたままのため、残すとスナップショットが取られ続ける)。

## E. AZ 障害

EBS は AZ を跨げないため、別の AZ に作り直して S3 から復元する。

1. `prod/app/server` の `public_subnet_ids[0]` を `[1]` に変更する (`main.tf` の `subnet_id` と `remote-resources.tf` の `data.aws_subnet.app_server` の 2 箇所)
2. [D](#d-データ用-ebs-の喪失) の手順を行う (EBS も新しい AZ に作成される)
3. 障害の復旧後、元の AZ へ戻すかを判断する (戻す場合は同じ手順を逆に行う)

## F. EBS スナップショットから復元 (最終手段)

S3 のバックアップが使えない場合のみ。**最大 24 時間分のデータを失う**。

```bash
# 1. 最新のスナップショットを確認する (DLM が日次 JST 03:00 に取得)
aws-vault exec narinote -- aws ec2 describe-snapshots --owner-ids self \
  --filters Name=tag:Snapshot,Values=nari-note-postgres-data \
  --query 'reverse(sort_by(Snapshots,&StartTime))[:3].[SnapshotId,StartTime]' --output table

# 2. スナップショットから EC2 と同じ AZ にボリュームを作る (AZ はコンソールか describe-instances で確認)
aws-vault exec narinote -- aws ec2 create-volume --snapshot-id snap-xxxx \
  --availability-zone ap-northeast-1a --volume-type gp3 \
  --tag-specifications 'ResourceType=volume,Tags=[{Key=Name,Value=nari-note-postgres-data},{Key=Snapshot,Value=nari-note-postgres-data}]'

# 3. Terraform の管理を付け替えて apply する (EC2 が再作成される)
cd infrastruture/terraform/prod/app/server
aws-vault exec narinote -- terragrunt state rm aws_ebs_volume.postgres_data
aws-vault exec narinote -- terragrunt import aws_ebs_volume.postgres_data vol-xxxx
aws-vault exec narinote -- terragrunt apply
```

新しい EC2 は既存のデータを検出してそのまま起動し、PostgreSQL がクラッシュリカバリで整合状態に戻す。動作確認後、EC2 上で S3 側のバックアップを立て直す。

```bash
sudo nari-note-db-backup base
```

## トラブルシューティング

### restore が失敗した

アプリは停止したまま終了する。原因はログで確認する。

```bash
sudo journalctl -u postgresql -n 100 --no-pager
```

| メッセージ | 原因 | 対応 |
| --- | --- | --- |
| `recovery ended before configured recovery target was reached` | 指定時刻まで WAL がアーカイブされていない (未来の時刻・アーカイブ滞留中の時刻) | より前の時刻で restore し直す |
| `指定時刻より前に完了したベースバックアップがありません` | 保持期間 (約 7 日) より前、または導入前の時刻を指定した | 保持期間内の時刻を指定する。それより前は [F](#f-ebs-スナップショットから復元-最終手段) |
| `前回の restore が finalize されていません` | 前回の restore の退避データが残っている | finalize するか、[restore を取り消す](#restore-を取り消して元のデータに戻す) |

### restore を取り消して元のデータに戻す

退避データ (`/data/postgresql/.pre-restore-<時刻>`) を元に戻す。finalize 前であれば可能。
`/data/postgresql` は postgres ユーザー以外が入れない (700) ため、root のシェルで行う。

```bash
sudo -i

systemctl stop postgresql
cd /data/postgresql
STASH=$(ls -d .pre-restore-*)
COLOR=$(sed -n 's/.*color=\([^ ]*\).*/\1/p' /var/lib/nari-note-db-backup/pending-restore)

# 復元したデータを捨てて、退避していた元のデータを戻す
install -d -o postgres -g postgres -m 700 .discarded-restore
find . -mindepth 1 -maxdepth 1 ! -name '.pre-restore-*' ! -name .discarded-restore -exec mv -t .discarded-restore {} +
find "${STASH}" -mindepth 1 -maxdepth 1 -exec mv -t . {} +
rmdir "${STASH}"
rm -rf .discarded-restore
rm -f /var/lib/nari-note-db-backup/pending-restore

systemctl start postgresql
systemctl start "nari-note-backend@${COLOR}"
exit
```

取り消した restore で分岐したタイムラインは S3 に残るが、restore はベースバックアップと同じタイムラインを辿る (`recovery_target_timeline = 'current'`) ため、以降の restore には影響しない。

### WAL アーカイブ滞留のアラームが鳴った

```bash
sudo -u postgres psql -c 'SELECT * FROM pg_stat_archiver;'                 # last_failed_time / last_failed_wal
sudo journalctl -u postgresql -n 100 --no-pager | grep -i archive         # wal-push のエラー内容
sudo find /data/postgresql/pg_wal/archive_status -name '*.ready' | wc -l  # 未送信数
```

S3 や IAM の問題が解消すれば、PostgreSQL が自動で再送する。未送信の WAL は `pg_wal` に溜まり続けるため、`df -h /data/postgresql` で空き容量も確認する。

### ベースバックアップ欠落のアラームが鳴った

```bash
sudo systemctl status nari-note-db-backup-base.timer nari-note-db-backup-base.service --no-pager
sudo journalctl -u nari-note-db-backup-base.service -n 50 --no-pager
sudo nari-note-db-backup base   # 原因を解消したら手動で取得する
```

## 訓練

月 1 回程度、dev 環境で以下を行い、手順とスクリプトが動くことを確認する。

1. dev の DB に確認用の行を入れ、時刻を控える
2. その行を削除する
3. [B](#b-db-全体を時刻-t-へ戻す) で控えた時刻に戻し、行が復活することを確認する
4. [C](#c-一部の行テーブルだけ戻す-本番無停止) で同じ行を抽出できることを確認する
