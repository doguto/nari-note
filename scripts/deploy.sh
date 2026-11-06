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

APP_NAME="${APP_NAME:-nari-note}"
S3_BUCKET="${S3_BUCKET:-${APP_NAME}-deploy}"
AWS_REGION="ap-northeast-1"
AWS_VAULT_PROFILE="${AWS_VAULT_PROFILE:-narinote}"
RUNTIME="linux-arm64"
BINARY_NAME="nari-note-backend"

# shellcheck source=lib/ssm.sh
source "${SCRIPT_DIR}/lib/ssm.sh"

# S3 キーにコミットハッシュと日時を含めてトレーサビリティを確保
GIT_SHA=$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "unknown")
TIMESTAMP=$(date -u +"%Y%m%d%H%M%S")
S3_RELEASE_PREFIX="releases/${TIMESTAMP}-${GIT_SHA}"
S3_KEY="${S3_RELEASE_PREFIX}/${BINARY_NAME}"
S3_LATEST_KEY="latest/${BINARY_NAME}"

PUBLISH_DIR="${PROJECT_DIR}/bin/publish"
# スクリプト自体もリポジトリの最新版で毎回上書きし、EC2 起動時点の版に固定されないようにする
DEPLOY_SCRIPT="${REPO_ROOT}/infrastruture/terraform/modules/app/server/nari-note-deploy.sh"

INSTANCE_ID=$(resolve_instance_id)

echo "=== nari-note-backend デプロイ ==="
echo "  Git SHA   : ${GIT_SHA}"
echo "  S3 Bucket : s3://${S3_BUCKET}"
echo "  S3 Key    : ${S3_KEY}"
echo "  Instance  : ${INSTANCE_ID}"
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
# EC2 はリリース単位のプレフィックスから取得するため、同時に別のデプロイが latest を更新しても混ざらない
aws_cli s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_KEY}" --no-progress
for SETTINGS_FILE in appsettings.json appsettings.Production.json; do
  aws_cli s3 cp "${PROJECT_DIR}/${SETTINGS_FILE}" "s3://${S3_BUCKET}/${S3_RELEASE_PREFIX}/${SETTINGS_FILE}" --no-progress
done
aws_cli s3 cp "${DEPLOY_SCRIPT}" "s3://${S3_BUCKET}/${S3_RELEASE_PREFIX}/nari-note-deploy.sh" --no-progress

echo "[3/4] latest を更新中..."
# latest はインスタンス再作成時に userdata が取得する
aws_cli s3 cp "${BINARY_PATH}" "s3://${S3_BUCKET}/${S3_LATEST_KEY}" --no-progress
for SETTINGS_FILE in appsettings.json appsettings.Production.json; do
  aws_cli s3 cp "${PROJECT_DIR}/${SETTINGS_FILE}" "s3://${S3_BUCKET}/latest/${SETTINGS_FILE}" --no-progress
done

echo "[4/4] EC2 に Blue/Green デプロイ中 (SSM Run Command: ${INSTANCE_ID})..."
# 待機系に配置 → ヘルスチェック → nginx 切り替え → 旧系統停止 は EC2 上の nari-note-deploy が行う
REMOTE_SCRIPT=$(cat <<EOF
set -euo pipefail
WORK_DIR=\$(mktemp -d)
trap 'rm -rf "\${WORK_DIR}"' EXIT
cd "\${WORK_DIR}"
aws s3 cp --recursive "s3://${S3_BUCKET}/${S3_RELEASE_PREFIX}/" . --region ${AWS_REGION} --no-progress
install -m 755 ./nari-note-deploy.sh /usr/local/bin/nari-note-deploy
/usr/local/bin/nari-note-deploy deploy ./${BINARY_NAME} ./appsettings.json ./appsettings.Production.json
/usr/local/bin/nari-note-deploy status
EOF
)
ssm_run "${INSTANCE_ID}" "deploy ${TIMESTAMP}-${GIT_SHA}" "${REMOTE_SCRIPT}"

echo ""
echo "=== デプロイ完了 ==="
echo "  リリース : s3://${S3_BUCKET}/${S3_RELEASE_PREFIX}/"
echo "  latest   : s3://${S3_BUCKET}/${S3_LATEST_KEY}"
echo "  EC2      : ${INSTANCE_ID}"
echo "  ロールバック: ./scripts/rollback.sh"
