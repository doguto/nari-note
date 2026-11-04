#!/bin/bash
# Blue/Green の待機系 (直前のバージョン) へ nginx の向き先を戻す
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# .env があれば読み込む
ENV_FILE="${SCRIPT_DIR}/.env"
if [[ -f "${ENV_FILE}" ]]; then
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
fi

EC2_USER="ec2-user"
EC2_HOST="${EC2_HOST:?EC2_HOST が未設定やわ。例: EC2_HOST=1.2.3.4 ./scripts/rollback.sh}"
SSH_KEY="${SSH_KEY:-${HOME}/.ssh/nari-note}"

echo "=== nari-note-backend ロールバック (${EC2_USER}@${EC2_HOST}) ==="
ssh -i "${SSH_KEY}" \
    -o StrictHostKeyChecking=no \
    -o ConnectTimeout=10 \
    "${EC2_USER}@${EC2_HOST}" \
    "sudo /usr/local/bin/nari-note-deploy rollback && sudo /usr/local/bin/nari-note-deploy status"
