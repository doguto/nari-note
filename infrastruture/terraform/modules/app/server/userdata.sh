#!/bin/bash
set -e

# == Swap ==
# メモリ急増時に即 OOM Killer が発動しないための保険。常時使われるようならインスタンスサイズを見直す
# ルートが XFS のため fallocate ではなく dd で作成する。dnf update 等でのメモリ急増にも備えて最初に有効化する
if [ ! -f /swapfile ]; then
  dd if=/dev/zero of=/swapfile bs=1M count=1024
  chmod 600 /swapfile
  mkswap /swapfile
fi
swapon --show | grep -q /swapfile || swapon /swapfile
grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap defaults 0 0' >> /etc/fstab

echo 'vm.swappiness = 10' > /etc/sysctl.d/90-swap.conf
sysctl -p /etc/sysctl.d/90-swap.conf

# AL2023 のリポジトリは AMI のリリースバージョンに固定されており、そのままでは新しい更新が配信されない
# セキュリティ更新を受け取れるよう、常に最新のリリースを参照させる
echo 'latest' > /etc/dnf/vars/releasever

# パッケージの更新
dnf update -y

# 設定ファイルを S3 から取得 (userdata の上限 16KB を超えないよう、埋め込まずに Terraform が S3 へ配置している)
CONFIG_DIR="/var/tmp/nari-note-server-config"
mkdir -p "$${CONFIG_DIR}"
aws s3 cp --recursive "s3://${app_name}-deploy/${server_config_prefix}/" "$${CONFIG_DIR}/" \
  --region ap-northeast-1

# == dnf-automatic ==
# PostgreSQL を含む OS のセキュリティ更新を毎日自動で適用し、必要な時だけ OS を再起動する
dnf install -y dnf-automatic
install -m 644 "$${CONFIG_DIR}/dnf-automatic.conf" /etc/dnf/automatic.conf
mkdir -p /etc/systemd/system/dnf-automatic.timer.d
install -m 644 "$${CONFIG_DIR}/dnf-automatic.timer.conf" /etc/systemd/system/dnf-automatic.timer.d/override.conf
systemctl daemon-reload
systemctl enable --now dnf-automatic.timer

# == nginx ==
dnf install -y nginx

# nginx のリバースプロキシ設定を配置
install -m 644 "$${CONFIG_DIR}/nari-note-backend.nginx.conf" /etc/nginx/conf.d/nari-note-backend.conf

# Blue/Green の向き先ファイル。nginx の起動に必要なため、アプリ配置前に blue を向けた状態で用意しておく
# (以降は nari-note-deploy が書き換える)
mkdir -p /etc/nginx/nari-note-backend
cat > /etc/nginx/nari-note-backend/active-upstream.conf <<'EOF'
# active: blue
# このファイルは nari-note-deploy が書き換える。手で編集しないこと
server 127.0.0.1:5001;
EOF

# Cloudflare Origin 証明書を SSM から取得して設置 (nginx 起動前に必要)
mkdir -p /etc/nginx/ssl
chmod 700 /etc/nginx/ssl

aws ssm get-parameter \
  --name "/${app_name}/nginx/cloudflare-origin-cert" \
  --with-decryption \
  --query "Parameter.Value" \
  --output text \
  --region ap-northeast-1 \
  > /etc/nginx/ssl/cloudflare-origin.crt

aws ssm get-parameter \
  --name "/${app_name}/nginx/cloudflare-origin-key" \
  --with-decryption \
  --query "Parameter.Value" \
  --output text \
  --region ap-northeast-1 \
  > /etc/nginx/ssl/cloudflare-origin.key

chmod 600 /etc/nginx/ssl/cloudflare-origin.key

# フロントエンドプロキシ検証用の共有シークレットを SSM から取得し、nginx 設定へ埋め込む
INTERNAL_API_KEY=$(aws ssm get-parameter \
  --name "/${app_name}/nginx/internal-api-key" \
  --with-decryption \
  --query "Parameter.Value" \
  --output text \
  --region ap-northeast-1)

sed -i "s|__INTERNAL_API_KEY__|$${INTERNAL_API_KEY}|" /etc/nginx/conf.d/nari-note-backend.conf

# nginx の自動起動を有効化&起動
systemctl enable --now nginx

# == CloudWatch Agent ==
dnf install -y amazon-cloudwatch-agent

install -m 644 "$${CONFIG_DIR}/amazon-cloudwatch-agent.json" /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json

/opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
  -a fetch-config \
  -m ec2 \
  -s \
  -c file:/opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json


# == PostgreSQL ==
# contrib は shared_preload_libraries で読み込む pg_stat_statements に必要。無いと PostgreSQL が起動しない
dnf install -y postgresql17 postgresql17-server postgresql17-contrib

PGDATA="/data/postgresql"

# PostgreSQL データ用の EBS ボリュームを特定し、未フォーマットなら初期化してマウントする
# (インスタンスの user_data 変更による再作成時にもデータを失わないよう、ルートボリュームとは独立させている)
# NOTE: ルートボリュームも /dev/disk/by-id/nvme-Amazon_Elastic_Block_Store_* にマッチするため、ワイルドカードではなく Terraform から渡した実際のボリュームIDで一意に特定する
DEVICE="/dev/disk/by-id/nvme-Amazon_Elastic_Block_Store_${postgres_volume_id}"
if [ ! -e "$${DEVICE}" ]; then
  echo "PostgreSQL data volume (EBS) not found: $${DEVICE}" >&2
  exit 1
fi

mkdir -p "$${PGDATA}"

if ! blkid "$${DEVICE}" >/dev/null 2>&1; then
  mkfs.xfs "$${DEVICE}"
  udevadm settle
fi

VOLUME_UUID=$(blkid -s UUID -o value "$${DEVICE}")
if [ -z "$${VOLUME_UUID}" ]; then
  echo "Failed to read UUID for $${DEVICE}" >&2
  exit 1
fi
grep -q "UUID=$${VOLUME_UUID} " /etc/fstab || echo "UUID=$${VOLUME_UUID} $${PGDATA} xfs defaults,nofail 0 2" >> /etc/fstab

mountpoint -q "$${PGDATA}" || mount "$${PGDATA}"

chown postgres:postgres "$${PGDATA}"
chmod 700 "$${PGDATA}"

# データディレクトリを EBS ボリューム上のパスに向ける
mkdir -p /etc/systemd/system/postgresql.service.d
cat > /etc/systemd/system/postgresql.service.d/override.conf <<EOF
[Service]
Environment=PGDATA=$${PGDATA}
EOF
systemctl daemon-reload

# ストレージ上のページ破損を読み込み時に検知できるよう data checksums を有効にする
if [ ! -f "$${PGDATA}/PG_VERSION" ]; then
  PGDATA="$${PGDATA}" PGSETUP_INITDB_OPTIONS="--data-checksums" /usr/bin/postgresql-setup --initdb
fi

# checksums 無しで initdb された既存クラスタは、PostgreSQL の起動前 (停止中) に有効化する
# 異常終了したクラスタでは pg_checksums が失敗するため、その場合は起動を優先して後で手動で有効化する
if sudo -u postgres /usr/bin/pg_controldata "$${PGDATA}" | grep -q '^Data page checksum version:[[:space:]]*0$'; then
  sudo -u postgres /usr/bin/pg_checksums --enable --progress -D "$${PGDATA}" || \
    echo "Failed to enable data checksums on $${PGDATA}. Enable them manually with pg_checksums." >&2
fi

# 設定ファイルは initdb 済みの既存データボリュームにも変更が反映されるよう、毎回上書きする
# 外部ネットワークには一切公開せず、localhost からの接続のみ許可する
install -o postgres -g postgres -m 600 "$${CONFIG_DIR}/pg_hba.conf" "$${PGDATA}/pg_hba.conf"

# postgresql.conf 本体は initdb の生成物のまま残し、上書きしたい設定だけを conf.d に配置する
PG_CONF_DIR="$${PGDATA}/conf.d"
install -d -o postgres -g postgres -m 700 "$${PG_CONF_DIR}"
grep -q "^include_dir = 'conf.d'" "$${PGDATA}/postgresql.conf" || \
  echo "include_dir = 'conf.d'" >> "$${PGDATA}/postgresql.conf"

install -o postgres -g postgres -m 600 "$${CONFIG_DIR}/postgresql.conf" "$${PG_CONF_DIR}/nari-note.conf"

# WAL-G・バックアップ用 timer を配置する。archive_command が WAL-G を使うため PostgreSQL の起動前に行う
aws s3 cp "s3://${app_name}-deploy/${db_backup_script_key}" /usr/local/bin/nari-note-db-backup   --region ap-northeast-1
chmod 755 /usr/local/bin/nari-note-db-backup
/usr/local/bin/nari-note-db-backup install "${app_name}" "${db_backup_bucket_name}"

systemctl enable --now postgresql

# アプリ用のロール・データベースを作成 (冪等)
DB_USERNAME=$(aws ssm get-parameter \
  --name "/${app_name}/db/username" \
  --with-decryption \
  --query "Parameter.Value" \
  --output text \
  --region ap-northeast-1)

DB_PASSWORD=$(aws ssm get-parameter \
  --name "/${app_name}/db/password" \
  --with-decryption \
  --query "Parameter.Value" \
  --output text \
  --region ap-northeast-1)

sudo -u postgres psql -tc "SELECT 1 FROM pg_roles WHERE rolname = '$${DB_USERNAME}'" | grep -q 1 || \
  sudo -u postgres psql -c "CREATE ROLE \"$${DB_USERNAME}\" WITH LOGIN PASSWORD '$${DB_PASSWORD}';"

# バックアップ等の postgres ユーザーの作業を止めないよう、サーバー全体ではなくアプリ用ロールにのみ設定する
sudo -u postgres psql -c "ALTER ROLE \"$${DB_USERNAME}\" SET statement_timeout = '15s';"

sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname = 'nari_note'" | grep -q 1 || \
  sudo -u postgres psql -c "CREATE DATABASE nari_note OWNER \"$${DB_USERNAME}\";"

# 統計の収集自体はクラスタ全体で行われるが、参照用のビュー・関数は DB ごとに作成が必要
sudo -u postgres psql -d nari_note -c "CREATE EXTENSION IF NOT EXISTS pg_stat_statements;"


# == Application ==
dnf install -y aspnetcore-runtime-9.0

APP_NAME="nari-note-backend"
S3_BUCKET="${app_name}-deploy"
AWS_REGION="ap-northeast-1"
BOOTSTRAP_DIR="/var/tmp/$${APP_NAME}-bootstrap"

# 専用ユーザーを作成
useradd --system --no-create-home --shell /sbin/nologin "$${APP_NAME}" || true

# Blue/Green 用のテンプレートユニットを配置 (nari-note-backend@blue / nari-note-backend@green)
install -m 644 "$${CONFIG_DIR}/$${APP_NAME}@.service" "/etc/systemd/system/$${APP_NAME}@.service"
systemctl daemon-reload

# Blue/Green デプロイスクリプトを配置 (以降のデプロイでは scripts/deploy.sh がリポジトリの最新版で上書きする)
aws s3 cp "s3://$${S3_BUCKET}/${deploy_script_key}" /usr/local/bin/nari-note-deploy \
  --region "$${AWS_REGION}"
chmod 755 /usr/local/bin/nari-note-deploy

# S3 から最新のバイナリと設定を取得し、デプロイスクリプト経由で配置・起動する
mkdir -p "$${BOOTSTRAP_DIR}"
for FILE in "$${APP_NAME}" appsettings.json appsettings.Production.json; do
  aws s3 cp "s3://$${S3_BUCKET}/latest/$${FILE}" "$${BOOTSTRAP_DIR}/$${FILE}" \
    --region "$${AWS_REGION}"
done

# 初回は旧系統が動いていないため、ドレイン待ちは不要
DRAIN_SECONDS=0 /usr/local/bin/nari-note-deploy deploy \
  "$${BOOTSTRAP_DIR}/$${APP_NAME}" \
  "$${BOOTSTRAP_DIR}/appsettings.json" \
  "$${BOOTSTRAP_DIR}/appsettings.Production.json"

for FILE in "$${APP_NAME}" appsettings.json appsettings.Production.json; do
  rm -f "$${BOOTSTRAP_DIR}/$${FILE}"
done
rmdir "$${BOOTSTRAP_DIR}"
rm -rf "$${CONFIG_DIR}"

# dnf のパッケージキャッシュを削除しディスク容量を回収する
dnf clean all
