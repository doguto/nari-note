#!/bin/bash
# Lambda に配る zip を作る。Lambda と同じ Amazon Linux のイメージの中でやねうら王をビルドし、評価関数と Python の依存を同梱する
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
EVAL_FILE="${EVAL_FILE:-${HERE}/engine/eval/nn.bin}"
YANEURAOU_REF="${YANEURAOU_REF:-master}"
OUT_DIR="${HERE}/build"
BUILD_IMAGE="public.ecr.aws/lambda/python:3.13"

if [[ ! -f "${EVAL_FILE}" ]]; then
  echo "Error: 評価関数が見つかりません: ${EVAL_FILE} (EVAL_FILE で指定してください)" >&2
  exit 1
fi

mkdir -p "${OUT_DIR}"
rm -f "${OUT_DIR}/nari-note-engine.zip"

# Git Bash では -v のパスを Windows 形式で渡す
to_host_path() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else echo "$1"; fi
}

MSYS_NO_PATHCONV=1 docker run --rm --platform linux/amd64 \
  --entrypoint /bin/bash \
  -e YANEURAOU_REF="${YANEURAOU_REF}" \
  -v "$(to_host_path "${HERE}"):/src:ro" \
  -v "$(to_host_path "$(dirname "${EVAL_FILE}")"):/eval:ro" \
  -e EVAL_NAME="$(basename "${EVAL_FILE}")" \
  -v "$(to_host_path "${OUT_DIR}"):/out" \
  "${BUILD_IMAGE}" -c '
set -euo pipefail
PKG=$(command -v dnf || command -v microdnf)
"${PKG}" install -y gcc-c++ make git zip >/dev/null

git clone --quiet --depth 1 --branch "${YANEURAOU_REF}" https://github.com/yaneurao/YaneuraOu.git /tmp/yaneuraou
echo "YaneuraOu: ${YANEURAOU_REF} $(git -C /tmp/yaneuraou rev-parse --short HEAD)"
make -C /tmp/yaneuraou/source -j"$(nproc)" normal COMPILER=g++ YANEURAOU_EDITION=YANEURAOU_ENGINE_NNUE TARGET_CPU=AVX2 >/dev/null

mkdir -p /tmp/pkg/engine/eval
cp /tmp/yaneuraou/source/YaneuraOu-by-gcc /tmp/pkg/engine/
cp "/eval/${EVAL_NAME}" /tmp/pkg/engine/eval/nn.bin
pip install --quiet --root-user-action=ignore --disable-pip-version-check --target /tmp/pkg -r /src/requirements.txt
cp /src/lambda_function.py /src/service.py /src/usi.py /src/judge.py /src/candidates.py /src/position.py /tmp/pkg/

cd /tmp/pkg
zip -q -r -9 /out/nari-note-engine.zip .
'

echo "Built: ${OUT_DIR}/nari-note-engine.zip ($(du -h "${OUT_DIR}/nari-note-engine.zip" | cut -f1))"
