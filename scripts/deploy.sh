#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_DIR="${REPO_ROOT}/nari-note-backend"
PROJECT_FILE="${PROJECT_DIR}/nari-note-backend.csproj"

S3_BUCKET="nari-note-deploy"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="narinote"
RUNTIME="linux-x64"
BINARY_NAME="nari-note-backend"

# S3 キーにコミットハッシュと日時を含めてトレーサビリティを確保
GIT_SHA=$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "unknown")
TIMESTAMP=$(date -u +"%Y%m%d%H%M%S")
S3_KEY="releases/${TIMESTAMP}-${GIT_SHA}/${BINARY_NAME}"
S3_LATEST_KEY="latest/${BINARY_NAME}"

PUBLISH_DIR="${PROJECT_DIR}/bin/publish"

echo "=== nari-note-backend デプロイ ==="
echo "  Git SHA   : ${GIT_SHA}"
echo "  S3 Bucket : s3://${S3_BUCKET}"
echo "  S3 Key    : ${S3_KEY}"
echo ""

echo "[1/3] ビルド中 (runtime: ${RUNTIME})..."
dotnet publish "${PROJECT_FILE}" \
  --configuration Release \
  --runtime "${RUNTIME}" \
  --self-contained true \
  --output "${PUBLISH_DIR}" \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false \
  --nologo \
  -v minimal

BINARY_PATH="${PUBLISH_DIR}/${BINARY_NAME}"

if [[ ! -f "${BINARY_PATH}" ]]; then
  echo "Error: バイナリが見つかりませんでした: ${BINARY_PATH}" >&2
  exit 1
fi

echo "[1/3] ビルド完了: $(du -sh "${BINARY_PATH}" | cut -f1)"

echo "[2/3] S3 にアップロード中..."
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_KEY}" \
  --region "${AWS_REGION}" \
  --no-progress

echo "[3/3] latest を更新中..."
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_LATEST_KEY}" \
  --region "${AWS_REGION}" \
  --no-progress

echo ""
echo "=== デプロイ完了 ==="
echo "  リリース : s3://${S3_BUCKET}/${S3_KEY}"
echo "  latest   : s3://${S3_BUCKET}/${S3_LATEST_KEY}"
