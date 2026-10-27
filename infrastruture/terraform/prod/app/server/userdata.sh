#!/bin/bash
set -e

# パッケージの更新
dnf update -y

# == nginx ==
dnf install -y nginx

# nginx のリバースプロキシ設定を配置
cat > /etc/nginx/conf.d/nari-note-backend.conf <<'EOF'
${nginx_conf_file}
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

cat > /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json <<'EOF'
${cloudwatch_conf}
EOF

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

if [ ! -f "$${PGDATA}/PG_VERSION" ]; then
  PGDATA="$${PGDATA}" /usr/bin/postgresql-setup --initdb
fi

# 設定ファイルは initdb 済みの既存データボリュームにも変更が反映されるよう、毎回上書きする
# 外部ネットワークには一切公開せず、localhost からの接続のみ許可する
cat > "$${PGDATA}/pg_hba.conf" <<'HBA'
${pg_hba_conf}
HBA

# postgresql.conf 本体は initdb の生成物のまま残し、上書きしたい設定だけを conf.d に配置する
PG_CONF_DIR="$${PGDATA}/conf.d"
install -d -o postgres -g postgres -m 700 "$${PG_CONF_DIR}"
grep -q "^include_dir = 'conf.d'" "$${PGDATA}/postgresql.conf" || \
  echo "include_dir = 'conf.d'" >> "$${PGDATA}/postgresql.conf"

cat > "$${PG_CONF_DIR}/nari-note.conf" <<'CONF'
${postgresql_conf}
CONF
chown postgres:postgres "$${PG_CONF_DIR}/nari-note.conf"
chmod 600 "$${PG_CONF_DIR}/nari-note.conf"

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

# S3 からバイナリを取得
APP_NAME="nari-note-backend"
S3_BUCKET="${app_name}-deploy"
AWS_REGION="ap-northeast-1"
INSTALL_DIR="/opt/$${APP_NAME}"

mkdir -p "$${INSTALL_DIR}"
aws s3 cp "s3://$${S3_BUCKET}/latest/$${APP_NAME}" "$${INSTALL_DIR}/$${APP_NAME}" \
  --region "$${AWS_REGION}"
chmod +x "$${INSTALL_DIR}/$${APP_NAME}"
aws s3 cp "s3://$${S3_BUCKET}/latest/appsettings.Production.json" "$${INSTALL_DIR}/appsettings.Production.json" \
  --region "$${AWS_REGION}"

# 専用ユーザーを作成
useradd --system --no-create-home --shell /sbin/nologin "$${APP_NAME}" || true
chown "$${APP_NAME}:$${APP_NAME}" "$${INSTALL_DIR}/$${APP_NAME}"
chown "$${APP_NAME}:$${APP_NAME}" "$${INSTALL_DIR}/appsettings.Production.json"

# ログディレクトリを作成
mkdir -p /var/log/$${APP_NAME}
chown "$${APP_NAME}:$${APP_NAME}" /var/log/$${APP_NAME}

# systemd サービスを配置
cat > /etc/systemd/system/$${APP_NAME}.service <<'EOF'
${service_file}
EOF

systemctl daemon-reload
systemctl enable --now "$${APP_NAME}"

# dnf のパッケージキャッシュを削除しディスク容量を回収する
dnf clean all
