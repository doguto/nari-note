#!/bin/bash
# PostgreSQL のバックアップ (WAL-G) とリストアを EC2 上で行うスクリプト
#
# WAL は PostgreSQL の archive_command から WAL-G が S3 へ随時送る。
# このスクリプトはその周辺 (導入・日次ベースバックアップ・監視・リストア) を担う。
# 手順の全体像は infrastruture/docs/db-backup-recovery.md を参照。
#
# 使い方:
#   nari-note-db-backup install <app_name> <bucket>
#       WAL-G・設定・systemd timer を配置する (冪等)。userdata から呼ばれる
#   nari-note-db-backup base
#       ベースバックアップを取得し、ページ破損が無いことを確認する (systemd timer から日次で呼ばれる)
#   nari-note-db-backup check
#       PostgreSQL の死活・ページ破損の検知件数・ディスク使用率・未アーカイブの WAL 数を CloudWatch へ送る (systemd timer から 5 分毎に呼ばれる)
#   nari-note-db-backup list
#       S3 上のベースバックアップ一覧を表示する
#   nari-note-db-backup restore <latest|時刻> [-y]
#       アプリと PostgreSQL を止め、S3 から指定時刻まで復元する。時刻はオフセット付きで指定する
#       (例: '2026-11-05 14:31:00+09:00')。復元前のデータは PGDATA 配下に退避し、アプリは停止したままにする
#   nari-note-db-backup finalize [-y]
#       restore 後にデータを確認してから実行する。退避データを削除し、ベースバックアップを取得してアプリを起動する
#   nari-note-db-backup extract <時刻>
#       本番を止めずに、指定時刻の DB を port 5433 で一時起動する (一部の行だけ戻す用途)
#   nari-note-db-backup extract-stop
#       extract で起動した一時 DB を停止・削除する
set -euo pipefail

WALG_VERSION="v3.0.9"
# Ubuntu 20.04 向けビルドは glibc 2.31 以上で動くため、AL2023 (glibc 2.34) でそのまま使える
WALG_ASSET="wal-g-pg-20.04-aarch64"
WALG_SHA256="89f0cea535eed8c5b742358bff5151e65b302eab90689d0afb38750f8d505936"
WALG_BIN="/usr/local/bin/wal-g"
WALG_CONFIG="/etc/wal-g/config.json"

ENV_FILE="/etc/nari-note-db-backup.env"
STATE_DIR="/var/lib/nari-note-db-backup"
PENDING_RESTORE_FILE="${STATE_DIR}/pending-restore"
# mtime が最後にベースバックアップが成功した時刻。dnf-automatic が更新前の確認に使う
LAST_BASE_SUCCESS_FILE="${STATE_DIR}/last-base-success"
LOCK_FILE="/run/nari-note-db-backup.lock"
SELF="/usr/local/bin/nari-note-db-backup"

PGDATA="@PGDATA@"
STAGING_DIR="${PGDATA}/.restore-staging"
STASH_PREFIX=".pre-restore-"
EXTRACT_DIR="/var/tmp/nari-note-pg-extract"
EXTRACT_PORT=5433
PG_CTL="/usr/bin/pg_ctl"
# 使用率を監視するファイルシステム。ルートは swap・dnf キャッシュ・extract の一時 DB 展開先を兼ねる
DISK_MOUNT_PATHS=("/" "${PGDATA}")
# Terraform が配布時に置換する (enable_db_health_check)。アラームの無い環境ではメトリクス送信を止める
ENABLE_HEALTH_CHECK="@ENABLE_HEALTH_CHECK@"

APP_UNIT="nari-note-backend"
NGINX_UPSTREAM_FILE="/etc/nginx/nari-note-backend/active-upstream.conf"
AWS_REGION="ap-northeast-1"
RECOVERY_TIMEOUT_SECONDS=1800

log() {
  echo "[nari-note-db-backup] $*"
}

die() {
  echo "[nari-note-db-backup] Error: $*" >&2
  exit 1
}

load_env() {
  [[ -f "${ENV_FILE}" ]] || die "${ENV_FILE} がありません。先に install を実行してください"
  # shellcheck disable=SC1090
  source "${ENV_FILE}"
}

walg() {
  sudo -u postgres "${WALG_BIN}" --config "${WALG_CONFIG}" "$@"
}

psql_at() {
  local port="$1"
  shift
  sudo -u postgres psql -X -At -p "${port}" -d postgres "$@"
}

put_metric() {
  local name="$1" value="$2" unit="$3" extra_dims="${4:-}"
  aws cloudwatch put-metric-data \
    --region "${AWS_REGION}" \
    --namespace CWAgent \
    --metric-name "${name}" \
    --value "${value}" \
    --unit "${unit}" \
    --dimensions "AppName=${APP_NAME}${extra_dims:+,${extra_dims}}"
}

acquire_lock() {
  exec 9>"${LOCK_FILE}"
  flock -n 9 || die "別のバックアップ・リストア処理が実行中です"
}

confirm() {
  local message="$1" assume_yes="$2"
  [[ "${assume_yes}" == "yes" ]] && return 0
  local answer=""
  read -r -p "[nari-note-db-backup] ${message} [y/N]: " answer || true
  [[ "${answer}" == "y" || "${answer}" == "Y" ]] || die "中止しました"
}

# PostgreSQL にもそのまま渡すため、タイムゾーンの曖昧さが無いようオフセット付きの形式に限定する
validate_target_time() {
  local target="$1"
  [[ "${target}" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}[\ T][0-9]{2}:[0-9]{2}:[0-9]{2}([+-][0-9]{2}:[0-9]{2}|Z)$ ]] \
    || die "時刻はオフセット付きで指定してください (例: '2026-11-05 14:31:00+09:00')"
  date -d "${target}" +%s >/dev/null 2>&1 || die "時刻を解釈できません: ${target}"
}

# 指定時刻より前に完了したベースバックアップのうち最新のものを選ぶ
# (完了前のベースバックアップからは、その完了時点より前には戻せないため)
choose_backup() {
  local target="$1"
  if [[ "${target}" == "latest" ]]; then
    echo "LATEST"
    return
  fi

  local target_epoch
  target_epoch="$(date -d "${target}" +%s)"
  walg backup-list --detail --json | python3 -c '
import datetime, json, re, sys

target = int(sys.argv[1])
best = None
for backup in json.load(sys.stdin) or []:
    # Go の RFC3339Nano はナノ秒まで出力されるが、Python 3.9 の fromisoformat はマイクロ秒までしか解釈できない
    finished = re.sub(r"\.\d+", "", backup["finish_time"]).replace("Z", "+00:00")
    finished_epoch = datetime.datetime.fromisoformat(finished).timestamp()
    if finished_epoch < target and (best is None or finished_epoch > best[0]):
        best = (finished_epoch, backup["backup_name"])
if best is None:
    sys.exit(1)
print(best[1])
' "${target_epoch}" || die "指定時刻より前に完了したベースバックアップがありません"
}

write_recovery_conf() {
  local data_dir="$1" target="$2" extra="${3:-}"
  local conf_dir="${data_dir}/conf.d"
  install -d -o postgres -g postgres -m 700 "${conf_dir}"

  {
    echo "# nari-note-db-backup が復元のために一時的に配置する。復元完了後に削除される"
    echo "restore_command = '${WALG_BIN} --config ${WALG_CONFIG} wal-fetch %f %p'"
    echo "recovery_target_action = 'promote'"
    # 既定の latest だと、取り消した restore で分岐したタイムラインが S3 に残っている場合にそちらを辿ってしまう。
    # finalize で分岐直後にベースバックアップを取るため、ベースバックアップと同じタイムラインを辿れば常に正しい履歴になる
    echo "recovery_target_timeline = 'current'"
    if [[ "${target}" != "latest" ]]; then
      echo "recovery_target_time = '${target}'"
    fi
    if [[ -n "${extra}" ]]; then
      echo "${extra}"
    fi
  } > "${conf_dir}/zz-recovery.conf"
  chown postgres:postgres "${conf_dir}/zz-recovery.conf"
  chmod 600 "${conf_dir}/zz-recovery.conf"

  sudo -u postgres touch "${data_dir}/recovery.signal"
}

# WAL の再生が終わり、promote されるまで待つ
wait_promoted() {
  local port="$1" is_alive_cmd="$2"
  local deadline=$((SECONDS + RECOVERY_TIMEOUT_SECONDS))

  log "WAL の再生完了を待機中..."
  until [[ "$(psql_at "${port}" -c 'SELECT pg_is_in_recovery()' 2>/dev/null || true)" == "f" ]]; do
    if ! eval "${is_alive_cmd}"; then
      return 1
    fi
    if (( SECONDS >= deadline )); then
      log "${RECOVERY_TIMEOUT_SECONDS} 秒以内に再生が完了しませんでした"
      return 1
    fi
    sleep 3
  done
  log "WAL の再生完了、promote 済み"
}

active_color() {
  if [[ -f "${NGINX_UPSTREAM_FILE}" ]]; then
    sed -n 's/^# active: \(blue\|green\)$/\1/p' "${NGINX_UPSTREAM_FILE}"
  fi
}

write_unit() {
  local name="$1" content="$2"
  printf '%s\n' "${content}" > "/etc/systemd/system/${name}"
}

cmd_install() {
  local app_name="$1" bucket="$2"

  if [[ "$("${WALG_BIN}" --version 2>/dev/null || true)" != *"${WALG_VERSION}"* ]]; then
    log "WAL-G ${WALG_VERSION} をインストールします"
    local tmp
    tmp="$(mktemp)"
    curl -fsSL --retry 3 -o "${tmp}" \
      "https://github.com/wal-g/wal-g/releases/download/${WALG_VERSION}/${WALG_ASSET}"
    echo "${WALG_SHA256}  ${tmp}" | sha256sum -c --quiet - || { rm -f "${tmp}"; die "WAL-G のチェックサムが一致しません"; }
    install -m 755 "${tmp}" "${WALG_BIN}"
    rm -f "${tmp}"
  fi

  cat > "${ENV_FILE}" <<EOF
APP_NAME=${app_name}
DB_BACKUP_BUCKET=${bucket}
EOF

  install -d -m 750 -g postgres /etc/wal-g
  cat > "${WALG_CONFIG}" <<EOF
{
  "WALG_S3_PREFIX": "s3://${bucket}/wal-g",
  "AWS_REGION": "${AWS_REGION}",
  "WALG_COMPRESSION_METHOD": "zstd",
  "WALG_UPLOAD_CONCURRENCY": "1",
  "WALG_VERIFY_PAGE_CHECKSUMS": "true",
  "PGHOST": "/run/postgresql"
}
EOF
  chown root:postgres "${WALG_CONFIG}"
  chmod 640 "${WALG_CONFIG}"

  install -d -m 700 "${STATE_DIR}"

  # restore 中に PostgreSQL を勝手に起動しないよう、Requires= ではなく After= だけにしている
  write_unit nari-note-db-backup-base.service "[Unit]
Description=nari-note PostgreSQL base backup (WAL-G)
After=postgresql.service

[Service]
Type=oneshot
ExecStart=${SELF} base
Nice=10
IOSchedulingClass=idle"

  write_unit nari-note-db-backup-base.timer "[Unit]
Description=Daily nari-note PostgreSQL base backup

[Timer]
# JST 03:15。DLM の EBS スナップショット (JST 03:00) と重ならないようずらす
OnCalendar=*-*-* 18:15:00 UTC
Persistent=true

[Install]
WantedBy=timers.target"

  if [[ "${ENABLE_HEALTH_CHECK}" != "true" ]]; then
    systemctl disable --now nari-note-db-backup-check.timer 2>/dev/null || true
    rm -f /etc/systemd/system/nari-note-db-backup-check.service /etc/systemd/system/nari-note-db-backup-check.timer
    systemctl daemon-reload
    systemctl enable --now nari-note-db-backup-base.timer
    log "install 完了 (bucket: ${bucket}, check 無効)"
    return
  fi

  write_unit nari-note-db-backup-check.service "[Unit]
Description=Report nari-note PostgreSQL health metrics
After=postgresql.service

[Service]
Type=oneshot
ExecStart=${SELF} check"

  write_unit nari-note-db-backup-check.timer "[Unit]
Description=Report nari-note PostgreSQL health metrics every 5 minutes

[Timer]
OnCalendar=*:0/5

[Install]
WantedBy=timers.target"

  systemctl daemon-reload
  systemctl enable --now nari-note-db-backup-base.timer nari-note-db-backup-check.timer
  log "install 完了 (bucket: ${bucket})"
}

# WAL-G はページ破損を検知しても backup-push を成功扱いで終え、破損ブロックを files_metadata.json の CorruptBlocks に記録するだけのため、
# ここで確認して失敗扱いにする (成功を記録しなければベースバックアップ欠落のアラームで気付ける)
verify_latest_backup_pages() {
  local backup corrupt
  backup="$(walg backup-list --detail --json | python3 -c '
import datetime, json, re, sys

latest = None
for backup in json.load(sys.stdin) or []:
    finished = re.sub(r"\.\d+", "", backup["finish_time"]).replace("Z", "+00:00")
    finished_epoch = datetime.datetime.fromisoformat(finished).timestamp()
    if latest is None or finished_epoch > latest[0]:
        latest = (finished_epoch, backup["backup_name"])
if latest is None:
    sys.exit(1)
print(latest[1])
')" || { log "取得したベースバックアップを特定できませんでした"; return 1; }

  corrupt="$(aws s3 cp --region "${AWS_REGION}" --quiet \
    "s3://${DB_BACKUP_BUCKET}/wal-g/basebackups_005/${backup}/files_metadata.json" - | python3 -c '
import json, sys

for path, desc in sorted((json.load(sys.stdin).get("Files") or {}).items()):
    info = desc.get("CorruptBlocks")
    if info:
        print("%s: %d blocks (%s)" % (path, info["CorruptBlocksCount"], info["SomeCorruptBlocks"]))
')" || { log "${backup} の files_metadata.json を確認できませんでした"; return 1; }

  if [[ -n "${corrupt}" ]]; then
    log "ベースバックアップ ${backup} でページ破損を検知しました"
    printf '%s\n' "${corrupt}" >&2
    return 1
  fi
}

run_base_backup() {
  systemctl is-active --quiet postgresql || die "PostgreSQL が起動していません"
  log "ベースバックアップを取得します"
  walg backup-push "${PGDATA}"
  verify_latest_backup_pages || die "ベースバックアップの検証に失敗しました"
  touch "${LAST_BASE_SUCCESS_FILE}"
  put_metric DbBackupSuccess 1 Count "Type=base"
  log "ベースバックアップ完了"
}

cmd_base() {
  load_env
  acquire_lock
  run_base_backup
}

# 読み込み時に PostgreSQL が検知したページ破損の件数 (data checksums)。累計値で、pg_stat_reset() まで減らない
report_checksum_failures() {
  local checksum_failures
  checksum_failures="$(psql_at 5432 -c 'SELECT coalesce(sum(checksum_failures), 0) FROM pg_stat_database')" \
    || { log "checksum_failures を取得できませんでした"; return 1; }
  [[ "${checksum_failures}" =~ ^[0-9]+$ ]] || { log "checksum_failures を解釈できませんでした: ${checksum_failures}"; return 1; }
  put_metric DbChecksumFailures "${checksum_failures}" Count || { log "DbChecksumFailures の送信に失敗しました"; return 1; }
}

# 経過時間ではなく未送信の WAL 数を見る。アクセスが無いと WAL が切り替わらず、
# 最終アーカイブ時刻からの経過時間は正常時でも伸び続けるため
cmd_check() {
  load_env
  # 1 つの失敗で残りのメトリクスまで欠損すると原因を切り分けにくいため、失敗しても続行し、最後に終了コードで失敗を残す
  local failed=0

  local db_up=0
  if sudo -u postgres pg_isready -q -h /run/postgresql -p 5432 -t 10; then
    db_up=1
  fi
  put_metric DbUp "${db_up}" Count || { log "DbUp の送信に失敗しました"; failed=1; }

  if (( db_up == 1 )); then
    report_checksum_failures || failed=1
  fi

  local mount_path used_percent
  for mount_path in "${DISK_MOUNT_PATHS[@]}"; do
    # 未マウントだとルートの使用率を拾ってしまうため送らない (アラームはデータ欠損として発報する)
    if ! mountpoint -q "${mount_path}"; then
      log "${mount_path} がマウントされていないため、ディスク使用率の送信をスキップします"
      failed=1
      continue
    fi
    used_percent="$(df --output=pcent "${mount_path}" | tail -n 1 | tr -dc '0-9' || true)"
    if [[ -z "${used_percent}" ]]; then
      log "${mount_path} のディスク使用率を取得できませんでした"
      failed=1
      continue
    fi
    put_metric DiskUsedPercent "${used_percent}" Percent "MountPath=${mount_path}" \
      || { log "DiskUsedPercent (${mount_path}) の送信に失敗しました"; failed=1; }
  done

  local pending
  pending="$(find "${PGDATA}/pg_wal/archive_status" -name '*.ready' 2>/dev/null | wc -l)"
  put_metric DbWalArchivePendingFiles "${pending}" Count \
    || { log "DbWalArchivePendingFiles の送信に失敗しました"; failed=1; }

  return "${failed}"
}

cmd_list() {
  walg backup-list --pretty --detail
}

cmd_restore() {
  local target="$1" assume_yes="$2"
  [[ "${target}" == "latest" ]] || validate_target_time "${target}"
  mountpoint -q "${PGDATA}" || die "${PGDATA} に EBS ボリュームがマウントされていません"
  [[ ! -f "${PENDING_RESTORE_FILE}" ]] || die "前回の restore が finalize されていません: $(cat "${PENDING_RESTORE_FILE}")"

  load_env
  local backup
  backup="$(choose_backup "${target}")"

  log "復元先時刻: ${target} / ベースバックアップ: ${backup}"
  confirm "アプリと PostgreSQL を停止して復元します。よろしいですか?" "${assume_yes}"
  acquire_lock

  local color
  color="$(active_color)"
  log "アプリを停止します (稼働系統: ${color:-なし})"
  systemctl stop "${APP_UNIT}@blue" "${APP_UNIT}@green"
  log "PostgreSQL を停止します"
  systemctl stop postgresql

  rm -rf "${STAGING_DIR}"
  install -d -o postgres -g postgres -m 700 "${STAGING_DIR}"
  log "ベースバックアップを取得します (失敗した場合、既存データは変更されていません)"
  walg backup-fetch "${STAGING_DIR}" "${backup}"

  # PGDATA は EBS のマウントポイントそのものでディレクトリごと移動できないため、中身を同一ボリューム内で入れ替える
  local stash="${PGDATA}/${STASH_PREFIX}$(date -u +%Y%m%dT%H%M%SZ)"
  install -d -o postgres -g postgres -m 700 "${stash}"
  find "${PGDATA}" -mindepth 1 -maxdepth 1 \
    ! -name "${STASH_PREFIX}*" ! -name "$(basename "${STAGING_DIR}")" \
    -exec mv -t "${stash}" {} +
  find "${STAGING_DIR}" -mindepth 1 -maxdepth 1 -exec mv -t "${PGDATA}" {} +
  rmdir "${STAGING_DIR}"
  log "復元前のデータを ${stash} に退避しました"

  write_recovery_conf "${PGDATA}" "${target}"
  echo "stash=${stash} color=${color:-blue} target=${target}" > "${PENDING_RESTORE_FILE}"

  log "PostgreSQL を起動し、WAL を再生します"
  # 指定時刻まで WAL が揃っていない場合等は起動に失敗するため、成否は wait_promoted で判定する
  systemctl start postgresql || true
  if ! wait_promoted 5432 "systemctl is-active --quiet postgresql"; then
    journalctl -u postgresql -n 50 --no-pager >&2 || true
    # Restart=on-failure により失敗した recovery が繰り返されるのを止める
    systemctl stop postgresql || true
    die "復元に失敗しました。退避データ: ${stash} (アプリは停止したままです)"
  fi
  rm -f "${PGDATA}/conf.d/zz-recovery.conf"

  log "復元完了。アプリは停止したままです"
  log "データを確認後 'sudo ${SELF} finalize' を実行してください"
  log "  確認例: sudo -u postgres psql -d nari_note"
}

cmd_finalize() {
  local assume_yes="$1"
  [[ -f "${PENDING_RESTORE_FILE}" ]] || die "finalize 待ちの restore がありません"
  load_env

  local stash color
  stash="$(sed -n 's/.*stash=\([^ ]*\).*/\1/p' "${PENDING_RESTORE_FILE}")"
  color="$(sed -n 's/.*color=\([^ ]*\).*/\1/p' "${PENDING_RESTORE_FILE}")"
  [[ "$(psql_at 5432 -c 'SELECT pg_is_in_recovery()')" == "f" ]] || die "PostgreSQL がまだリカバリ中です"

  confirm "退避データ ${stash} を削除し、アプリ (${color}) を起動します。よろしいですか?" "${assume_yes}"
  acquire_lock

  # 退避データは PGDATA 配下にあり、残したままだとベースバックアップに含まれてしまうため先に消す
  rm -rf "${PGDATA:?}/${STASH_PREFIX}"*
  # 復元によりタイムラインが分岐したため、新しいタイムラインの起点となるベースバックアップを即座に取る
  run_base_backup

  log "アプリ (${color}) を起動します"
  systemctl start "${APP_UNIT}@${color}"
  rm -f "${PENDING_RESTORE_FILE}"
  log "finalize 完了"
}

cmd_extract() {
  local target="$1"
  validate_target_time "${target}"
  [[ ! -e "${EXTRACT_DIR}" ]] || die "${EXTRACT_DIR} が既にあります。先に extract-stop を実行してください"

  load_env
  local backup
  backup="$(choose_backup "${target}")"
  log "復元先時刻: ${target} / ベースバックアップ: ${backup}"

  install -d -o postgres -g postgres -m 700 "${EXTRACT_DIR}"
  walg backup-fetch "${EXTRACT_DIR}" "${backup}"

  # 一時 DB の WAL が本番のアーカイブに混ざらないよう archive_mode を切る。メモリが少ないため shared_buffers も絞る
  write_recovery_conf "${EXTRACT_DIR}" "${target}" "port = ${EXTRACT_PORT}
archive_mode = off
shared_buffers = 32MB"

  log "一時 DB を port ${EXTRACT_PORT} で起動します"
  sudo -u postgres "${PG_CTL}" -D "${EXTRACT_DIR}" -l "${EXTRACT_DIR}/extract.log" -W start
  if ! wait_promoted "${EXTRACT_PORT}" "sudo -u postgres ${PG_CTL} -D ${EXTRACT_DIR} status >/dev/null"; then
    tail -n 50 "${EXTRACT_DIR}/extract.log" >&2 || true
    die "一時 DB の復元に失敗しました。'sudo ${SELF} extract-stop' で片付けてください"
  fi

  log "一時 DB の起動完了 (port ${EXTRACT_PORT})"
  log "  抽出例: sudo -u postgres pg_dump -p ${EXTRACT_PORT} -d nari_note -t <table> --data-only > rows.sql"
  log "  終了後: sudo ${SELF} extract-stop"
}

cmd_extract_stop() {
  [[ -e "${EXTRACT_DIR}" ]] || die "${EXTRACT_DIR} がありません"
  if sudo -u postgres "${PG_CTL}" -D "${EXTRACT_DIR}" status >/dev/null 2>&1; then
    sudo -u postgres "${PG_CTL}" -D "${EXTRACT_DIR}" -m fast stop
  fi
  rm -rf "${EXTRACT_DIR}"
  log "一時 DB を削除しました"
}

[[ "${EUID}" -eq 0 ]] || die "root で実行してください (sudo)"

assume_yes="no"
args=()
for arg in "$@"; do
  if [[ "${arg}" == "-y" ]]; then
    assume_yes="yes"
  else
    args+=("${arg}")
  fi
done
set -- "${args[@]+"${args[@]}"}"

case "${1:-}" in
  install)
    [[ $# -eq 3 ]] || die "使い方: $0 install <app_name> <bucket>"
    cmd_install "$2" "$3"
    ;;
  base) cmd_base ;;
  check) cmd_check ;;
  list) cmd_list ;;
  restore)
    [[ $# -eq 2 ]] || die "使い方: $0 restore <latest|時刻> [-y]"
    cmd_restore "$2" "${assume_yes}"
    ;;
  finalize) cmd_finalize "${assume_yes}" ;;
  extract)
    [[ $# -eq 2 ]] || die "使い方: $0 extract <時刻>"
    cmd_extract "$2"
    ;;
  extract-stop) cmd_extract_stop ;;
  *) die "使い方: $0 {install|base|check|list|restore|finalize|extract|extract-stop}" ;;
esac
