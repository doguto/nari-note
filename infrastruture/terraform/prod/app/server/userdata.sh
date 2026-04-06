#!/bin/bash
set -e

# パッケージの更新
dnf update -y

# nginx
dnf install -y nginx

# nginx のリバースプロキシ設定を配置
cat > /etc/nginx/conf.d/nari-note-backend.conf <<'EOF'
${nginx_conf_file}
EOF

# nginx の自動起動を有効化&起動
systemctl enable --now nginx


# Application
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

# 専用ユーザーを作成
useradd --system --no-create-home --shell /sbin/nologin "$${APP_NAME}" || true
chown "$${APP_NAME}:$${APP_NAME}" "$${INSTALL_DIR}/$${APP_NAME}"

# systemd サービスを配置
cat > /etc/systemd/system/$${APP_NAME}.service <<'EOF'
${service_file}
EOF

systemctl daemon-reload
systemctl enable --now "$${APP_NAME}"
