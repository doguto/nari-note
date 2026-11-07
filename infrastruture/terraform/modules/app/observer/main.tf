locals {
  instance_id = data.terraform_remote_state.server.outputs.instance_id
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

  dimensions = {
    InstanceId = local.instance_id
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-app-server-status-check-alarm"
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
