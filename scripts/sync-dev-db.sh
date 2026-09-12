#!/bin/bash
set -euo pipefail

# prod の RDS から現在のデータを取得し、dev 環境の PostgreSQL（app-server 同居）へ流し込む。
# prod への書き込みは一切行わない（pg_dump は読み取りのみ）。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

ENV_FILE="${SCRIPT_DIR}/.env"
if [[ -f "${ENV_FILE}" ]]; then
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
fi

AWS_REGION="ap-northeast-1"
SSH_KEY="${SSH_KEY:-${HOME}/.ssh/nari-note}"
EC2_USER="ec2-user"

PROD_APP_NAME="nari-note"
DEV_APP_NAME="nari-note-dev"
DB_NAME="nari_note"

PROD_HOST="${PROD_HOST:?PROD_HOST が未設定やわ。例: PROD_HOST=1.2.3.4 ./scripts/sync-dev-db.sh}"
DEV_HOST="${DEV_HOST:?DEV_HOST が未設定やわ。例: DEV_HOST=5.6.7.8 ./scripts/sync-dev-db.sh}"

SSH_OPTS=(-i "${SSH_KEY}" -o StrictHostKeyChecking=no -o ConnectTimeout=10)
REMOTE_DUMP_PATH="/tmp/nari_note_sync.dump"

# インスタンス再作成でホストキーが変わっていても弾かれないよう、古いエントリを事前に掃除する
ssh-keygen -R "${PROD_HOST}" >/dev/null 2>&1 || true
ssh-keygen -R "${DEV_HOST}" >/dev/null 2>&1 || true
LOCAL_DUMP_PATH="$(mktemp)"

cleanup() {
  rm -f "${LOCAL_DUMP_PATH}"
  ssh "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}" "rm -f ${REMOTE_DUMP_PATH}" || true
  ssh "${SSH_OPTS[@]}" "${EC2_USER}@${DEV_HOST}" "rm -f ${REMOTE_DUMP_PATH}" || true
}
trap cleanup EXIT

echo "=== prod -> dev DB 同期 ==="
echo "  prod: ${PROD_HOST}"
echo "  dev : ${DEV_HOST}"
echo ""

echo "[1/3] prod (${PROD_HOST}) で pg_dump 実行中..."
ssh "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}" "$(cat <<EOF
set -euo pipefail
command -v pg_dump >/dev/null 2>&1 || sudo dnf install -y postgresql17 >/dev/null
DB_HOST=\$(aws ssm get-parameter --name /${PROD_APP_NAME}/db/host --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_USER=\$(aws ssm get-parameter --name /${PROD_APP_NAME}/db/username --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_PASS=\$(aws ssm get-parameter --name /${PROD_APP_NAME}/db/password --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
PGPASSWORD="\${DB_PASS}" pg_dump -h "\${DB_HOST}" -U "\${DB_USER}" -d ${DB_NAME} --no-owner --no-privileges --clean --if-exists -f ${REMOTE_DUMP_PATH}
EOF
)"

echo "[2/3] dumpファイルを転送中 (prod -> local -> dev)..."
scp "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}:${REMOTE_DUMP_PATH}" "${LOCAL_DUMP_PATH}"
scp "${SSH_OPTS[@]}" "${LOCAL_DUMP_PATH}" "${EC2_USER}@${DEV_HOST}:${REMOTE_DUMP_PATH}"

echo "[3/3] dev (${DEV_HOST}) へ pg_restore 実行中..."
ssh "${SSH_OPTS[@]}" "${EC2_USER}@${DEV_HOST}" "$(cat <<EOF
set -euo pipefail
DB_USER=\$(aws ssm get-parameter --name /${DEV_APP_NAME}/db/username --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_PASS=\$(aws ssm get-parameter --name /${DEV_APP_NAME}/db/password --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
PGPASSWORD="\${DB_PASS}" psql -h 127.0.0.1 -U "\${DB_USER}" -d ${DB_NAME} -v ON_ERROR_STOP=1 -f ${REMOTE_DUMP_PATH}
sudo systemctl restart nari-note-backend
EOF
)"

echo ""
echo "=== 完了 ==="
