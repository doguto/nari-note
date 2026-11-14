#!/bin/bash
# dnf-automatic の後に実行する。userdata が /usr/local/sbin/nari-note-post-update に配置し、dnf-automatic.service.conf の ExecStartPost から呼ばれる
set -euo pipefail

# nari-note-db-backup と同じロック。実行中のベースバックアップ・リストアを PostgreSQL の更新・再起動で中断させない
LOCK_FILE="/run/nari-note-db-backup.lock"

log() {
  echo "[nari-note-post-update] $*"
}

exec 9>"${LOCK_FILE}"
if ! flock -n 9; then
  # 再起動が必要な状態は残るため、翌日の実行で再び判定される
  log "nari-note-db-backup の処理が実行中のため、PostgreSQL の更新・再起動を見送ります"
  exit 0
fi

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
