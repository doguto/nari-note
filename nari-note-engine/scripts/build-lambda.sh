#!/bin/bash
# Lambda に配る zip を作る。Lambda と同じ Amazon Linux のイメージの中で、評価関数のアーキテクチャごとにやねうら王をビルドし、評価関数と Python の依存を同梱する
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# src/service.py の ENGINES と同じ名前にする。NNUE のアーキテクチャはビルド時に埋め込むので、評価関数ごとにビルドする
ENGINES="tsuyouraou-mini:SFNN_ka2_4096_8_64_progress8 tsuyouraou-micro:SFNN_ka2_2048_8_64_progress8"
# 判定はエンジンの探索結果が変わらないことを前提にしているので、版は固定し、上げるときはここを書き換える
YANEURAOU_REF="${YANEURAOU_REF:-c046f30bef11527c84fd04a1ead5a47abfa910c8}"
OUT_DIR="${ROOT}/build"
BUILD_IMAGE="public.ecr.aws/lambda/python:3.13"

# SFNN の progress 付きのアーキテクチャは、nn.bin と同じ場所に progress.bin が無いと読み込めない
for entry in ${ENGINES}; do
  for file in nn.bin progress.bin; do
    eval_file="${ROOT}/engine/${entry%%:*}/eval/${file}"
    if [[ ! -f "${eval_file}" ]]; then
      echo "Error: 評価関数が見つかりません: ${eval_file}" >&2
      exit 1
    fi
  done
done

mkdir -p "${OUT_DIR}"
rm -f "${OUT_DIR}/nari-note-engine.zip"

# Git Bash では -v のパスを Windows 形式で渡す
to_host_path() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else echo "$1"; fi
}

MSYS_NO_PATHCONV=1 docker run --rm --platform linux/amd64 \
  --entrypoint /bin/bash \
  -e YANEURAOU_REF="${YANEURAOU_REF}" \
  -e ENGINES="${ENGINES}" \
  -v "$(to_host_path "${ROOT}"):/work:ro" \
  -v "$(to_host_path "${OUT_DIR}"):/out" \
  "${BUILD_IMAGE}" -c '
set -euo pipefail
PKG=$(command -v dnf || command -v microdnf)
"${PKG}" install -y gcc-c++ make git zip >/dev/null

# --branch はコミット SHA を受け付けないので、fetch で取得する
git init --quiet /tmp/yaneuraou
git -C /tmp/yaneuraou fetch --quiet --depth 1 https://github.com/yaneurao/YaneuraOu.git "${YANEURAOU_REF}"
git -C /tmp/yaneuraou checkout --quiet FETCH_HEAD
echo "YaneuraOu: ${YANEURAOU_REF} $(git -C /tmp/yaneuraou rev-parse --short HEAD)"

for entry in ${ENGINES}; do
  name="${entry%%:*}"
  arch="${entry#*:}"
  echo "Build: ${name} (${arch})"
  make -C /tmp/yaneuraou/source clean >/dev/null
  # 未知のアーキテクチャのヘッダーは Python で生成される
  make -C /tmp/yaneuraou/source -j"$(nproc)" normal COMPILER=g++ PYTHON=python3 \
    YANEURAOU_EDITION="YANEURAOU_ENGINE_${arch}" TARGET_CPU=AVX2 >/dev/null
  mkdir -p "/tmp/pkg/engine/${name}"
  cp /tmp/yaneuraou/source/YaneuraOu-by-gcc "/tmp/pkg/engine/${name}/"
  # 評価関数と一緒に配られたライセンス表記も同梱する
  cp -r "/work/engine/${name}/eval" "/tmp/pkg/engine/${name}/eval"
done
pip install --quiet --root-user-action=ignore --disable-pip-version-check --target /tmp/pkg -r /work/requirements.txt
# server.py はローカル用の HTTP サーバーなので同梱しない
find /work/src -maxdepth 1 -name "*.py" ! -name server.py -exec cp {} /tmp/pkg/ \;
# 同梱漏れは Lambda でだけ ImportError になるので、ここで読み込めることを確かめる
(cd /tmp/pkg && python3 -c "import lambda_function")

cd /tmp/pkg
zip -q -r -9 /out/nari-note-engine.zip .
'

echo "Built: ${OUT_DIR}/nari-note-engine.zip ($(du -h "${OUT_DIR}/nari-note-engine.zip" | cut -f1))"
