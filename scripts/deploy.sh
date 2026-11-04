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

S3_BUCKET="${S3_BUCKET:-nari-note-deploy}"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="${AWS_VAULT_PROFILE:-narinote}"
RUNTIME="linux-x64"
BINARY_NAME="nari-note-backend"

EC2_USER="ec2-user"
EC2_HOST="${EC2_HOST:?EC2_HOST が未設定やわ。例: EC2_HOST=1.2.3.4 ./scripts/deploy.sh}"
SSH_KEY="${SSH_KEY:-${HOME}/.ssh/nari-note}"

# インスタンス再作成でホストキーが変わっていても弾かれないよう、古いエントリを事前に掃除する
ssh-keygen -R "${EC2_HOST}" >/dev/null 2>&1 || true

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
  -p:IncludeNativeLibrariesForSelfExtract=true \
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
for SETTINGS_FILE in appsettings.json appsettings.Production.json; do
  aws-vault exec "${AWS_VAULT_PROFILE}" -- aws s3 cp "${PROJECT_DIR}/${SETTINGS_FILE}" "s3://${S3_BUCKET}/latest/${SETTINGS_FILE}" \
    --region "${AWS_REGION}" \
    --no-progress
done

echo "[4/4] EC2 に Blue/Green デプロイ中 (${EC2_USER}@${EC2_HOST})..."
# 待機系に配置 → ヘルスチェック → nginx 切り替え → 旧系統停止 は EC2 上の nari-note-deploy が行う
# スクリプト自体もリポジトリの最新版で毎回上書きし、EC2 起動時点の版に固定されないようにする
DEPLOY_SCRIPT="${REPO_ROOT}/infrastruture/terraform/prod/app/server/nari-note-deploy.sh"
REMOTE_DIR="/tmp/${BINARY_NAME}-deploy"

ssh -i "${SSH_KEY}" \
    -o StrictHostKeyChecking=no \
    -o ConnectTimeout=10 \
    "${EC2_USER}@${EC2_HOST}" \
    "mkdir -p ${REMOTE_DIR}"

scp -i "${SSH_KEY}" \
    -o StrictHostKeyChecking=no \
    -o ConnectTimeout=10 \
    "${PROJECT_DIR}/appsettings.json" \
    "${PROJECT_DIR}/appsettings.Production.json" \
    "${DEPLOY_SCRIPT}" \
    "${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/"

ssh -i "${SSH_KEY}" \
    -o StrictHostKeyChecking=no \
    -o ConnectTimeout=10 \
    "${EC2_USER}@${EC2_HOST}" \
    "set -e
     cd ${REMOTE_DIR}
     aws s3 cp s3://${S3_BUCKET}/${S3_LATEST_KEY} ./${BINARY_NAME} --region ${AWS_REGION} --no-progress
     sudo install -m 755 ./nari-note-deploy.sh /usr/local/bin/nari-note-deploy
     sudo /usr/local/bin/nari-note-deploy deploy ./${BINARY_NAME} ./appsettings.json ./appsettings.Production.json
     rm -f ./${BINARY_NAME} ./appsettings.json ./appsettings.Production.json ./nari-note-deploy.sh
     sudo /usr/local/bin/nari-note-deploy status"

echo ""
echo "=== デプロイ完了 ==="
echo "  リリース : s3://${S3_BUCKET}/${S3_KEY}"
echo "  latest   : s3://${S3_BUCKET}/${S3_LATEST_KEY}"
echo "  EC2      : ${EC2_USER}@${EC2_HOST}"
echo "  ロールバック: EC2_HOST=${EC2_HOST} ./scripts/rollback.sh"
