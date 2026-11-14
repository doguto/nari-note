#!/bin/bash
# dnf-automatic の後に実行する。userdata が /usr/local/sbin/nari-note-post-update に配置し、dnf-automatic.service.conf の ExecStartPost から呼ばれる
set -euo pipefail

log() {
  echo "[nari-note-post-update] $*"
}

# dnf-automatic はセキュリティ更新のみを適用するが、PostgreSQL はバグ修正のみのマイナーリリースも適用する (RDS の自動マイナーバージョンアップグレード相当)
dnf -y upgrade 'postgresql17*'

# パッケージを更新しても、稼働中のプロセスは再起動するまで古いバイナリ・ライブラリを使い続ける
services="$(dnf needs-restarting -s)"

if grep -qx 'postgresql.service' <<< "${services}"; then
  log "PostgreSQL を再起動します"
  systemctl restart postgresql
fi

# アプリと nginx は再起動中に API が応答しなくなるため、自動では再起動しない (アプリは次回デプロイ、OS 再起動時に反映される)
others="$(grep -vx 'postgresql.service' <<< "${services}" || true)"
if [[ -n "${others}" ]]; then
  log "再起動が必要なサービス: $(tr '\n' ' ' <<< "${others}")"
fi
