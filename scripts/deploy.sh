#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_DIR="${REPO_ROOT}/nari-note-backend"
PROJECT_FILE="${PROJECT_DIR}/nari-note-backend.csproj"

# .env があれば読み込む
ENV_FILE="${SCRIPT_DIR}/.env"
if [[ -f "${ENV_FILE}" ]]; then
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
fi

S3_BUCKET="nari-note-deploy"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="narinote"
RUNTIME="linux-x64"
BINARY_NAME="nari-note-backend"

EC2_USER="ec2-user"
EC2_HOST="${EC2_HOST:?EC2_HOST が未設定やわ。例: EC2_HOST=1.2.3.4 ./scripts/deploy.sh}"
SSH_KEY="${SSH_KEY:-${HOME}/.ssh/nari-note}"

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

echo "[1/4] ビルド中 (runtime: ${RUNTIME})..."
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

echo "[1/4] ビルド完了: $(du -sh "${BINARY_PATH}" | cut -f1)"

echo "[2/4] S3 にアップロード中..."
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_KEY}" \
  --region "${AWS_REGION}" \
  --no-progress

echo "[3/4] latest を更新中..."
aws-vault exec "${AWS_VAULT_PROFILE}" -- aws s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_LATEST_KEY}" \
  --region "${AWS_REGION}" \
  --no-progress

echo "[4/4] EC2 にデプロイ中 (${EC2_USER}@${EC2_HOST})..."
ssh -i "${SSH_KEY}" \
    -o StrictHostKeyChecking=no \
    -o ConnectTimeout=10 \
    "${EC2_USER}@${EC2_HOST}" \
    "aws s3 cp s3://${S3_BUCKET}/${S3_LATEST_KEY} /tmp/${BINARY_NAME} --region ${AWS_REGION} \
     && sudo install -o ${BINARY_NAME} -g ${BINARY_NAME} -m 755 /tmp/${BINARY_NAME} /opt/${BINARY_NAME}/${BINARY_NAME} \
     && rm /tmp/${BINARY_NAME} \
     && sudo systemctl restart ${BINARY_NAME} \
     && sudo systemctl is-active --quiet ${BINARY_NAME} && echo 'サービス起動確認OK'"

echo ""
echo "=== デプロイ完了 ==="
echo "  リリース : s3://${S3_BUCKET}/${S3_KEY}"
echo "  latest   : s3://${S3_BUCKET}/${S3_LATEST_KEY}"
echo "  EC2      : ${EC2_USER}@${EC2_HOST}"
