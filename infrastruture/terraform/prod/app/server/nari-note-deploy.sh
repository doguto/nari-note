#!/bin/bash
# nari-note-backend の Blue/Green デプロイを EC2 上で行うスクリプト
#
# blue / green の 2 系統を systemd のテンプレートユニット (nari-note-backend@.service) で動かし、
# nginx の upstream が向く先を書き換えて reload することで、無停止で切り替える。
#
# 使い方:
#   nari-note-deploy deploy <binary> <appsettings.json> <appsettings.Production.json>
#       待機系に新バージョンを配置・起動し、ヘルスチェックが通れば nginx を切り替えて旧系統を停止する
#   nari-note-deploy rollback
#       待機系に残っている前バージョンを起動し直し、nginx をそちらへ切り替える
#   nari-note-deploy status
#       現在の稼働系統と各系統の状態を表示する
#
# 環境変数:
#   DRAIN_SECONDS        切り替え後、旧系統を止めるまで処理中リクエストの完了を待つ秒数 (既定: 30)
#   HEALTH_CHECK_TIMEOUT 起動後のヘルスチェックを待つ最大秒数。起動時のマイグレーション分も含む (既定: 120)
set -euo pipefail

APP_NAME="nari-note-backend"
INSTALL_ROOT="/opt/${APP_NAME}"
ENV_DIR="/etc/${APP_NAME}"
NGINX_UPSTREAM_DIR="/etc/nginx/${APP_NAME}"
NGINX_UPSTREAM_FILE="${NGINX_UPSTREAM_DIR}/active-upstream.conf"
LOG_DIR="/var/log/${APP_NAME}"
LOCK_FILE="/run/${APP_NAME}-deploy.lock"

DRAIN_SECONDS="${DRAIN_SECONDS:-30}"
HEALTH_CHECK_TIMEOUT="${HEALTH_CHECK_TIMEOUT:-120}"

log() {
  echo "[nari-note-deploy] $*"
}

die() {
  echo "[nari-note-deploy] Error: $*" >&2
  exit 1
}

port_of() {
  case "$1" in
    blue) echo 5001 ;;
    green) echo 5002 ;;
    *) die "不明な系統です: $1" ;;
  esac
}

other_of() {
  case "$1" in
    blue) echo green ;;
    green) echo blue ;;
    *) die "不明な系統です: $1" ;;
  esac
}

# nginx の upstream 設定ファイルに書いた系統名を現在の稼働系統とみなす
active_color() {
  if [[ -f "${NGINX_UPSTREAM_FILE}" ]]; then
    sed -n 's/^# active: \(blue\|green\)$/\1/p' "${NGINX_UPSTREAM_FILE}"
  fi
}

write_upstream() {
  local color="$1"
  cat <<EOF
# active: ${color}
# このファイルは nari-note-deploy が書き換える。手で編集しないこと
server 127.0.0.1:$(port_of "${color}");
EOF
}

# 系統ごとのディレクトリ・環境変数ファイルを用意する (冪等)
ensure_layout() {
  install -d -m 755 "${ENV_DIR}" "${NGINX_UPSTREAM_DIR}"
  install -d -o "${APP_NAME}" -g "${APP_NAME}" -m 755 "${LOG_DIR}"

  local color
  for color in blue green; do
    install -d -o "${APP_NAME}" -g "${APP_NAME}" -m 755 "${INSTALL_ROOT}/${color}"
    # 2 系統が同時に動く間に同じログファイルを奪い合わないよう、Serilog の File シンク (WriteTo の 2 番目) の出力先を系統ごとに分ける
    cat > "${ENV_DIR}/${color}.env" <<EOF
ASPNETCORE_URLS=http://127.0.0.1:$(port_of "${color}")
Serilog__WriteTo__1__Args__path=${LOG_DIR}/app-${color}.log
EOF
  done

  # 初回はまだ稼働系統が無いため、blue を向けておく (nginx の設定読み込みに必要)
  if [[ ! -f "${NGINX_UPSTREAM_FILE}" ]]; then
    write_upstream blue > "${NGINX_UPSTREAM_FILE}"
  fi
}

wait_healthy() {
  local color="$1"
  local url="http://127.0.0.1:$(port_of "${color}")/health"
  local deadline=$((SECONDS + HEALTH_CHECK_TIMEOUT))

  log "${color} のヘルスチェック待機中 (${url})..."
  until curl -fsS --max-time 3 "${url}" >/dev/null 2>&1; do
    if ! systemctl is-active --quiet "${APP_NAME}@${color}"; then
      if [[ "$(systemctl show -p ActiveState --value "${APP_NAME}@${color}")" != "activating" ]]; then
        log "${color} のプロセスが起動していません"
        return 1
      fi
    fi
    if (( SECONDS >= deadline )); then
      log "${color} のヘルスチェックが ${HEALTH_CHECK_TIMEOUT} 秒以内に成功しませんでした"
      return 1
    fi
    sleep 2
  done
  log "${color} のヘルスチェック OK"
}

# 指定系統を起動して健全性を確認し、nginx をそちらへ向けて旧系統を止める
switch_to() {
  local target="$1"
  local previous
  previous="$(active_color)"

  log "${target} を起動します"
  systemctl restart "${APP_NAME}@${target}"

  if ! wait_healthy "${target}"; then
    journalctl -u "${APP_NAME}@${target}" -n 50 --no-pager >&2 || true
    systemctl stop "${APP_NAME}@${target}" || true
    die "${target} が健全にならなかったため切り替えを中止しました (稼働系統 ${previous:-なし} はそのままです)"
  fi

  log "nginx の向き先を ${previous:-なし} -> ${target} に切り替えます"
  local backup
  backup="$(mktemp)"
  cp "${NGINX_UPSTREAM_FILE}" "${backup}"
  write_upstream "${target}" > "${NGINX_UPSTREAM_FILE}.tmp"
  mv "${NGINX_UPSTREAM_FILE}.tmp" "${NGINX_UPSTREAM_FILE}"

  if ! nginx -t -q; then
    cp "${backup}" "${NGINX_UPSTREAM_FILE}"
    rm -f "${backup}"
    systemctl stop "${APP_NAME}@${target}" || true
    die "nginx の設定検証に失敗したため元に戻しました"
  fi
  rm -f "${backup}"
  # reload は旧ワーカーが処理中のリクエストを捌き切ってから終了するため、切り替え時に接続は落ちない
  systemctl reload nginx

  # 再起動時に稼働系統だけが立ち上がるようにする
  systemctl enable --quiet "${APP_NAME}@${target}"

  if [[ -n "${previous}" && "${previous}" != "${target}" ]]; then
    systemctl disable --quiet "${APP_NAME}@${previous}" || true
    if systemctl is-active --quiet "${APP_NAME}@${previous}"; then
      log "${previous} の処理中リクエスト完了を ${DRAIN_SECONDS} 秒待ってから停止します"
      sleep "${DRAIN_SECONDS}"
      systemctl stop "${APP_NAME}@${previous}"
    fi
  fi

  log "切り替え完了: 稼働系統 = ${target}"
}

cmd_deploy() {
  local binary="$1" settings="$2" settings_prod="$3"
  [[ -f "${binary}" ]] || die "バイナリが見つかりません: ${binary}"
  [[ -f "${settings}" ]] || die "設定ファイルが見つかりません: ${settings}"
  [[ -f "${settings_prod}" ]] || die "設定ファイルが見つかりません: ${settings_prod}"

  ensure_layout

  local active target
  active="$(active_color)"
  target="$(other_of "${active:-green}")"

  # 稼働中の系統を誤って上書きしないための保険
  [[ "${target}" != "${active}" ]] || die "配置先が稼働系統と同じになりました"

  log "稼働系統: ${active:-なし} / 配置先: ${target}"
  systemctl stop "${APP_NAME}@${target}" 2>/dev/null || true

  local dir="${INSTALL_ROOT}/${target}"
  install -o "${APP_NAME}" -g "${APP_NAME}" -m 755 "${binary}" "${dir}/${APP_NAME}"
  install -o "${APP_NAME}" -g "${APP_NAME}" -m 644 "${settings}" "${dir}/appsettings.json"
  install -o "${APP_NAME}" -g "${APP_NAME}" -m 644 "${settings_prod}" "${dir}/appsettings.Production.json"

  switch_to "${target}"
}

cmd_rollback() {
  local active target
  active="$(active_color)"
  [[ -n "${active}" ]] || die "稼働系統が特定できません"
  target="$(other_of "${active}")"
  [[ -x "${INSTALL_ROOT}/${target}/${APP_NAME}" ]] || die "${target} にロールバック先のバイナリがありません"

  log "ロールバック: ${active} -> ${target}"
  switch_to "${target}"
}

cmd_status() {
  local active color
  active="$(active_color)"
  echo "稼働系統: ${active:-なし}"
  for color in blue green; do
    printf '  %-5s port=%s state=%s\n' \
      "${color}" "$(port_of "${color}")" \
      "$(systemctl is-active "${APP_NAME}@${color}" 2>/dev/null || true)"
  done
}

[[ "${EUID}" -eq 0 ]] || die "root で実行してください (sudo)"

# 同時実行で両系統が入れ替わるのを防ぐ
exec 9>"${LOCK_FILE}"
flock -n 9 || die "別のデプロイが実行中です"

case "${1:-}" in
  deploy)
    [[ $# -eq 4 ]] || die "使い方: $0 deploy <binary> <appsettings.json> <appsettings.Production.json>"
    cmd_deploy "$2" "$3" "$4"
    ;;
  rollback) cmd_rollback ;;
  status) cmd_status ;;
  *) die "使い方: $0 {deploy|rollback|status}" ;;
esac
