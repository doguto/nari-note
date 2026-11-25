#!/bin/bash
# 将棋エンジンの Lambda を更新する。zip を S3 に置き、関数のコードをその zip に差し替える
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENGINE_DIR="${REPO_ROOT}/nari-note-engine"

# .env があれば読み込む
ENV_FILE="${SCRIPT_DIR}/.env"
if [[ -f "${ENV_FILE}" ]]; then
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
fi

APP_NAME="${APP_NAME:-nari-note}"
S3_BUCKET="${S3_BUCKET:-${APP_NAME}-deploy}"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="${AWS_VAULT_PROFILE:-narinote}"
FUNCTION_NAME="${APP_NAME}-shogi-engine"

aws_cli() {
  aws-vault exec "${AWS_VAULT_PROFILE}" -- aws --region "${AWS_REGION}" "$@"
}

GIT_SHA=$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "unknown")
TIMESTAMP=$(date -u +"%Y%m%d%H%M%S")
S3_KEY="engine/releases/${TIMESTAMP}-${GIT_SHA}/nari-note-engine.zip"
# Terraform で関数を作るときはこのキーを参照する
S3_LATEST_KEY="engine/latest/nari-note-engine.zip"
ZIP_PATH="${ENGINE_DIR}/build/nari-note-engine.zip"

echo "=== nari-note-engine デプロイ ==="
echo "  Git SHA  : ${GIT_SHA}"
echo "  S3 Key   : s3://${S3_BUCKET}/${S3_KEY}"
echo "  Function : ${FUNCTION_NAME}"
echo ""

echo "[1/3] ビルド中..."
"${ENGINE_DIR}/build-lambda.sh"

echo "[2/3] S3 にアップロード中..."
aws_cli s3 cp "${ZIP_PATH}" "s3://${S3_BUCKET}/${S3_KEY}" --no-progress
aws_cli s3 cp "${ZIP_PATH}" "s3://${S3_BUCKET}/${S3_LATEST_KEY}" --no-progress

if ! aws_cli lambda get-function --function-name "${FUNCTION_NAME}" >/dev/null 2>&1; then
  echo "[3/3] ${FUNCTION_NAME} がまだ無いため、コードの差し替えを飛ばします。Terraform (app/engine) を apply すると latest の zip で作成されます"
  exit 0
fi

echo "[3/3] Lambda のコードを差し替え中..."
aws_cli lambda update-function-code \
  --function-name "${FUNCTION_NAME}" \
  --s3-bucket "${S3_BUCKET}" \
  --s3-key "${S3_KEY}" \
  --query 'CodeSha256' --output text
aws_cli lambda wait function-updated-v2 --function-name "${FUNCTION_NAME}"

echo ""
echo "=== デプロイ完了 ==="
echo "  リリース : s3://${S3_BUCKET}/${S3_KEY}"
