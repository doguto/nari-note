#!/bin/bash
set -euo pipefail

# prod: RDSのスナップショットを取得 -> アプリ停止 -> RDSから現在のデータをdump
# -> ローカル(手元)に退避 -> app-server同居のPostgreSQLへ復元、まで行う。
# アプリの再起動と db/rdb・db/secret の terraform apply はこのスクリプトの範囲外
# (SSMの切替は人間がterragruntのplanを見てから実行すること)。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

ENV_FILE="${SCRIPT_DIR}/.env"
if [[ -f "${ENV_FILE}" ]]; then
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
fi

AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="${AWS_VAULT_PROFILE:-narinote}"
SSH_KEY="${SSH_KEY:-${HOME}/.ssh/nari-note}"
EC2_USER="ec2-user"

APP_NAME="nari-note"
DB_NAME="nari_note"
RDS_IDENTIFIER="${APP_NAME}-rdb"

PROD_HOST="${PROD_HOST:?PROD_HOST が未設定やわ。例: PROD_HOST=1.2.3.4 ./scripts/cutover-db.sh}"

SSH_OPTS=(-i "${SSH_KEY}" -o StrictHostKeyChecking=no -o ConnectTimeout=10)
REMOTE_DUMP_PATH="/tmp/nari_note_migration.dump"
TIMESTAMP="$(date -u +%Y%m%d%H%M%S)"
SNAPSHOT_ID="${RDS_IDENTIFIER}-pre-migration-${TIMESTAMP}"
LOCAL_DUMP_PATH="./nari_note_migration_${TIMESTAMP}.dump"

ssh-keygen -R "${PROD_HOST}" >/dev/null 2>&1 || true

echo "=== prod DB移行 ==="
echo "  prod       : ${PROD_HOST}"
echo "  snapshot   : ${SNAPSHOT_ID}"
echo "  local dump : ${LOCAL_DUMP_PATH}"
echo ""

echo "[1/4] RDSスナップショット作成中..."
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws rds create-db-snapshot \
  --db-instance-identifier "${RDS_IDENTIFIER}" \
  --db-snapshot-identifier "${SNAPSHOT_ID}" \
  --region "${AWS_REGION}" >/dev/null
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws rds wait db-snapshot-available \
  --db-snapshot-identifier "${SNAPSHOT_ID}" \
  --region "${AWS_REGION}"
echo "  完了: ${SNAPSHOT_ID}"

echo ""
echo "[2/4] prod (${PROD_HOST}) でアプリ停止 + pg_dump 実行中..."
echo "  *** ここからダウンタイム開始 ***"
ssh "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}" "$(cat <<EOF
set -euo pipefail
sudo systemctl stop ${APP_NAME}-backend
DB_HOST=\$(aws ssm get-parameter --name /${APP_NAME}/db/host --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_USER=\$(aws ssm get-parameter --name /${APP_NAME}/db/username --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_PASS=\$(aws ssm get-parameter --name /${APP_NAME}/db/password --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
PGPASSWORD="\${DB_PASS}" pg_dump -h "\${DB_HOST}" -U "\${DB_USER}" -d ${DB_NAME} --no-owner --no-privileges --clean --if-exists -f ${REMOTE_DUMP_PATH}
EOF
)"

echo ""
echo "[3/4] dumpファイルを手元に退避中..."
scp "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}:${REMOTE_DUMP_PATH}" "${LOCAL_DUMP_PATH}"

echo ""
echo "[4/4] prod ローカルPostgreSQLへ復元中..."
ssh "${SSH_OPTS[@]}" "${EC2_USER}@${PROD_HOST}" "$(cat <<EOF
set -euo pipefail
DB_USER=\$(aws ssm get-parameter --name /${APP_NAME}/db/username --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
DB_PASS=\$(aws ssm get-parameter --name /${APP_NAME}/db/password --with-decryption --query Parameter.Value --output text --region ${AWS_REGION})
PGPASSWORD="\${DB_PASS}" psql -h 127.0.0.1 -U "\${DB_USER}" -d ${DB_NAME} -v ON_ERROR_STOP=1 -f ${REMOTE_DUMP_PATH}
rm -f ${REMOTE_DUMP_PATH}
EOF
)"

echo ""
echo "=== データ移行 完了 ==="
echo "  RDSスナップショット: ${SNAPSHOT_ID}"
echo "  手元のdump         : ${LOCAL_DUMP_PATH}"
echo ""
