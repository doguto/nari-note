# shellcheck shell=bash
# SSM Run Command (AWS-RunShellScript) で app-server 上のスクリプトを実行する共通処理
# 読み込む前に AWS_REGION / AWS_VAULT_PROFILE / APP_NAME を定義しておくこと

aws_cli() {
  aws-vault exec "${AWS_VAULT_PROFILE}" -- aws --region "${AWS_REGION}" "$@"
}

# EC2_INSTANCE_ID が未指定なら、Name タグから稼働中の app-server を 1 台だけ特定する
resolve_instance_id() {
  if [[ -n "${EC2_INSTANCE_ID:-}" ]]; then
    echo "${EC2_INSTANCE_ID}"
    return
  fi

  local ids
  ids=$(aws_cli ec2 describe-instances \
    --filters "Name=tag:Name,Values=${APP_NAME}-app-server" "Name=instance-state-name,Values=running" \
    --query 'Reservations[].Instances[].InstanceId' \
    --output text | tr -d '\r')

  local count
  count=$(wc -w <<<"${ids}")
  if [[ "${count}" -ne 1 ]]; then
    echo "Error: ${APP_NAME}-app-server の稼働中インスタンスを 1 台に特定できませんでした (${count} 台: ${ids}). EC2_INSTANCE_ID で指定してください" >&2
    return 1
  fi
  echo "${ids}"
}

# 使い方: ssm_run <instance-id> <comment> <script>
# script は root で bash 実行される。終了コードは EC2 上のスクリプトの終了コードを返す
ssm_run() {
  local instance_id="$1"
  local comment="$2"
  local script="$3"

  # クォートの崩れを避けるため、スクリプトは base64 で渡して EC2 上で復元する
  local encoded
  encoded=$(printf '%s' "${script}" | base64 | tr -d '\n')

  local command_id
  command_id=$(aws_cli ssm send-command \
    --instance-ids "${instance_id}" \
    --document-name "AWS-RunShellScript" \
    --comment "${comment}" \
    --parameters "{\"commands\":[\"echo ${encoded} | base64 -d | bash\"],\"executionTimeout\":[\"900\"]}" \
    --query 'Command.CommandId' \
    --output text | tr -d '\r')
  echo "  SSM Command ID: ${command_id}"

  local status=""
  while true; do
    sleep 5
    # 送信直後は InvocationDoesNotExist が返ることがあるため、取得できるまで待つ
    status=$(aws_cli ssm get-command-invocation \
      --command-id "${command_id}" \
      --instance-id "${instance_id}" \
      --query 'Status' \
      --output text 2>/dev/null | tr -d '\r') || status="Pending"
    case "${status}" in
      Pending | InProgress | Delayed | Cancelling) continue ;;
      *) break ;;
    esac
  done

  aws_cli ssm get-command-invocation \
    --command-id "${command_id}" \
    --instance-id "${instance_id}" \
    --query 'StandardOutputContent' \
    --output text
  local stderr_content
  stderr_content=$(aws_cli ssm get-command-invocation \
    --command-id "${command_id}" \
    --instance-id "${instance_id}" \
    --query 'StandardErrorContent' \
    --output text)
  if [[ -n "${stderr_content}" ]]; then
    echo "${stderr_content}" >&2
  fi

  if [[ "${status}" != "Success" ]]; then
    echo "Error: SSM コマンドが ${status} で終了しました (Command ID: ${command_id})" >&2
    return 1
  fi
}
