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

APP_NAME="${APP_NAME:-nari-note}"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="${AWS_VAULT_PROFILE:-narinote}"

# shellcheck source=lib/ssm.sh
source "${SCRIPT_DIR}/lib/ssm.sh"

INSTANCE_ID=$(resolve_instance_id)

echo "=== nari-note-backend ロールバック (SSM Run Command: ${INSTANCE_ID}) ==="
ssm_run "${INSTANCE_ID}" "rollback" \
  "/usr/local/bin/nari-note-deploy rollback && /usr/local/bin/nari-note-deploy status"
