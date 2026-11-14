locals {
  instance_id = data.terraform_remote_state.server.outputs.instance_id

  # ディスク使用率を監視するファイルシステム (nari-note-db-backup の DISK_MOUNT_PATHS と揃える)
  disk_mount_paths = {
    root            = "/"
    postgresql_data = "/data/postgresql"
  }
}

resource "aws_sns_topic" "alerts" {
  name = "${var.app_name}-alerts"
}

resource "aws_sns_topic_subscription" "alerts_email" {
  topic_arn = aws_sns_topic.alerts.arn
  protocol  = "email"
  endpoint  = var.alert_email
}

resource "aws_cloudwatch_metric_alarm" "app_server_cpu" {
  alarm_name          = "${var.app_name}-app-server-cpu-utilization"
  alarm_description   = "EC2 instance CPU utilization exceeded threshold"
  namespace           = "AWS/EC2"
  metric_name         = "CPUUtilization"
  statistic           = "Average"
  period              = var.cpu_alarm_period
  evaluation_periods  = var.cpu_alarm_evaluation_periods
  threshold           = var.cpu_alarm_threshold
  comparison_operator = "GreaterThanThreshold"
  # EC2 の再作成後に observer を apply し忘れ、旧インスタンスを監視し続けている場合も発報させる
  treat_missing_data = "breaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-app-server-cpu-alarm"
  }
}

# EC2 のステータスチェック (システム / インスタンス) が失敗している。dnf-automatic による再起動後に OS が起動しない場合等を検知する
# 正常な再起動でも一時的に失敗し得るため、5 分連続で失敗した場合に発報する
resource "aws_cloudwatch_metric_alarm" "app_server_status_check" {
  alarm_name          = "${var.app_name}-app-server-status-check-failed"
  alarm_description   = "EC2 instance status check failed"
  namespace           = "AWS/EC2"
  metric_name         = "StatusCheckFailed"
  statistic           = "Maximum"
  period              = 60
  evaluation_periods  = 5
  threshold           = 1
  comparison_operator = "GreaterThanOrEqualToThreshold"
  # EC2 の再作成後に observer を apply し忘れ、旧インスタンスを監視し続けている場合も発報させる
  treat_missing_data = "breaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-app-server-status-check-alarm"
  }
}

# PostgreSQL・.NET アプリ・nginx が 1GiB に同居しており、Blue/Green 切替中はアプリのメモリが一時的に約 2 倍になる
# 切替は 1 分程度で終わるため、5 分平均が 2 回連続で超えた場合に発報する
resource "aws_cloudwatch_metric_alarm" "app_server_memory" {
  alarm_name          = "${var.app_name}-app-server-memory-used"
  alarm_description   = "EC2 instance memory usage exceeded threshold"
  namespace           = "CWAgent"
  metric_name         = "mem_used_percent"
  statistic           = "Average"
  period              = 300
  evaluation_periods  = 2
  threshold           = var.memory_used_percent_threshold
  comparison_operator = "GreaterThanThreshold"
  # EC2 の再作成後に observer を apply し忘れ、旧インスタンスを監視し続けている場合も発報させる
  treat_missing_data = "breaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-app-server-memory-alarm"
  }
}

# 日次ベースバックアップが 26 時間成功していない (1 時間毎に評価し、26 回連続でデータ無しなら発報)
resource "aws_cloudwatch_metric_alarm" "db_base_backup_missing" {
  alarm_name          = "${var.app_name}-db-base-backup-missing"
  alarm_description   = "PostgreSQL base backup (WAL-G) has not succeeded for 26 hours"
  namespace           = "CWAgent"
  metric_name         = "DbBackupSuccess"
  statistic           = "Sum"
  period              = 3600
  evaluation_periods  = 26
  datapoints_to_alarm = 26
  threshold           = 1
  comparison_operator = "LessThanThreshold"
  treat_missing_data  = "breaching"

  dimensions = {
    AppName = var.app_name
    Type    = "base"
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-db-base-backup-missing-alarm"
  }
}

# S3 へ未送信の WAL が溜まっている。チェック自体が止まった場合 (データ無し) も異常とみなす
resource "aws_cloudwatch_metric_alarm" "db_wal_archive_backlog" {
  alarm_name          = "${var.app_name}-db-wal-archive-backlog"
  alarm_description   = "PostgreSQL WAL segments are not being archived to S3"
  namespace           = "CWAgent"
  metric_name         = "DbWalArchivePendingFiles"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 2
  threshold           = var.wal_archive_pending_threshold
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "breaching"

  dimensions = {
    AppName = var.app_name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-db-wal-archive-backlog-alarm"
  }
}

# PostgreSQL が接続を受け付けない (pg_isready の失敗)。チェック自体が止まった場合 (データ無し) も異常とみなす
# check は 5 分毎で送信時刻が評価期間の境界をまたぐと 1 期間データが欠けることがあるため、2 回連続で発報する
resource "aws_cloudwatch_metric_alarm" "db_down" {
  alarm_name          = "${var.app_name}-db-down"
  alarm_description   = "PostgreSQL is not accepting connections"
  namespace           = "CWAgent"
  metric_name         = "DbUp"
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 2
  threshold           = 1
  comparison_operator = "LessThanThreshold"
  treat_missing_data  = "breaching"

  dimensions = {
    AppName = var.app_name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-db-down-alarm"
  }
}

# PostgreSQL がデータページの読み込み時にチェックサム不一致を検知した
# PostgreSQL 停止中は送られないため、欠損は異常扱いしない (停止・check の停止は db-down で検知する)
# 累計値のため、原因を調べて復旧した後に pg_stat_reset() でリセットするまで発報し続ける
resource "aws_cloudwatch_metric_alarm" "db_checksum_failures" {
  alarm_name          = "${var.app_name}-db-checksum-failures"
  alarm_description   = "PostgreSQL detected data page checksum failures"
  namespace           = "CWAgent"
  metric_name         = "DbChecksumFailures"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"

  dimensions = {
    AppName = var.app_name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-db-checksum-failures-alarm"
  }
}

# ディスク使用率がしきい値を超えた。データ用 EBS が埋まると PostgreSQL が停止する
resource "aws_cloudwatch_metric_alarm" "disk_used" {
  for_each = local.disk_mount_paths

  alarm_name          = "${var.app_name}-disk-used-${replace(each.key, "_", "-")}"
  alarm_description   = "Disk usage of ${each.value} exceeded threshold"
  namespace           = "CWAgent"
  metric_name         = "DiskUsedPercent"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 2
  threshold           = var.disk_used_percent_threshold
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "breaching"

  dimensions = {
    AppName   = var.app_name
    MountPath = each.value
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-disk-used-${replace(each.key, "_", "-")}-alarm"
  }
}

# PostgreSQL ログの ERROR / FATAL / PANIC が多発している (メトリクスは app/logger のメトリクスフィルタで生成)
# statement_timeout によるキャンセル等で単発の ERROR は起こり得るため、件数の閾値で判定する
resource "aws_cloudwatch_metric_alarm" "db_error_log" {
  alarm_name          = "${var.app_name}-db-error-log"
  alarm_description   = "PostgreSQL ERROR/FATAL/PANIC log entries exceeded threshold"
  namespace           = "${var.app_name}/PostgreSQL"
  metric_name         = "ErrorLogCount"
  statistic           = "Sum"
  period              = 300
  evaluation_periods  = 1
  threshold           = var.db_error_log_threshold
  comparison_operator = "GreaterThanOrEqualToThreshold"
  treat_missing_data  = "notBreaching"

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-db-error-log-alarm"
  }
}
