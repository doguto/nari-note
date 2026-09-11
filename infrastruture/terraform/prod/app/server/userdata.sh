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
# NOTE: パッケージ名・systemdユニット名は Amazon Linux 2023 のリポジトリ内容に依存するため、
# 実際の apply 時に想定通りかを確認すること (現状 postgresql16 / postgresql16-server を想定)
dnf install -y postgresql16 postgresql16-server

PGDATA="/data/postgresql"

# PostgreSQL データ用の EBS ボリュームを特定し、未フォーマットなら初期化してマウントする
# (インスタンスの user_data 変更による再作成時にもデータを失わないよう、ルートボリュームとは独立させている)
DEVICE=$(readlink -f /dev/disk/by-id/nvme-Amazon_Elastic_Block_Store_* 2>/dev/null | head -n1)
if [ -z "$${DEVICE}" ]; then
  echo "PostgreSQL data volume (EBS) not found" >&2
  exit 1
fi

mkdir -p "$${PGDATA}"

if ! blkid "$${DEVICE}" >/dev/null 2>&1; then
  mkfs.xfs "$${DEVICE}"
fi

VOLUME_UUID=$(blkid -s UUID -o value "$${DEVICE}")
grep -q "$${VOLUME_UUID}" /etc/fstab || echo "UUID=$${VOLUME_UUID} $${PGDATA} xfs defaults,nofail 0 2" >> /etc/fstab

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

  # 外部ネットワークには一切公開せず、localhost からの接続のみ許可する
  cat > "$${PGDATA}/pg_hba.conf" <<'HBA'
local   all             all                                     peer
host    all             all             127.0.0.1/32            scram-sha-256
host    all             all             ::1/128                 scram-sha-256
HBA

  cat >> "$${PGDATA}/postgresql.conf" <<'CONF'
listen_addresses = 'localhost'
CONF
fi

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

sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname = 'nari_note'" | grep -q 1 || \
  sudo -u postgres psql -c "CREATE DATABASE nari_note OWNER \"$${DB_USERNAME}\";"


# == Application ==
dnf install -y aspnetcore-runtime-9.0

# S3 からバイナリを取得
APP_NAME="nari-note-backend"
S3_BUCKET="nari-note-deploy"
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
