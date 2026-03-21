locals {
  instance_id = data.terraform_remote_state.ec2.outputs.instance_id
}

# APIアプリケーションのロググループ
resource "aws_cloudwatch_log_group" "api" {
  name              = "/nari-note/api"
  retention_in_days = 30

  tags = {
    Name = "${var.app_name}-api-log-group"
  }
}

# システムログのロググループ
resource "aws_cloudwatch_log_group" "system" {
  name              = "/nari-note/system"
  retention_in_days = 14

  tags = {
    Name = "${var.app_name}-system-log-group"
  }
}

# CPU使用率アラーム
resource "aws_cloudwatch_metric_alarm" "cpu_high" {
  alarm_name          = "${var.app_name}-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CPUUtilization"
  namespace           = "AWS/EC2"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "CPU使用率が80%を超えています"
  treat_missing_data  = "notBreaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  tags = {
    Name = "${var.app_name}-cpu-high-alarm"
  }
}

# ステータスチェックアラーム
resource "aws_cloudwatch_metric_alarm" "status_check" {
  alarm_name          = "${var.app_name}-status-check-failed"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "StatusCheckFailed"
  namespace           = "AWS/EC2"
  period              = 60
  statistic           = "Maximum"
  threshold           = 0
  alarm_description   = "EC2インスタンスのステータスチェックが失敗しました"
  treat_missing_data  = "notBreaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  tags = {
    Name = "${var.app_name}-status-check-alarm"
  }
}

# メモリ使用率アラーム（CloudWatchエージェントが必要）
resource "aws_cloudwatch_metric_alarm" "memory_high" {
  alarm_name          = "${var.app_name}-memory-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "mem_used_percent"
  namespace           = "CWAgent"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "メモリ使用率が80%を超えています"
  treat_missing_data  = "notBreaching"

  dimensions = {
    InstanceId = local.instance_id
  }

  tags = {
    Name = "${var.app_name}-memory-high-alarm"
  }
}

# ディスク使用率アラーム
resource "aws_cloudwatch_metric_alarm" "disk_high" {
  alarm_name          = "${var.app_name}-disk-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "disk_used_percent"
  namespace           = "CWAgent"
  period              = 300
  statistic           = "Average"
  threshold           = 85
  alarm_description   = "ディスク使用率が85%を超えています"
  treat_missing_data  = "notBreaching"

  dimensions = {
    InstanceId = local.instance_id
    path       = "/"
    fstype     = "xfs"
  }

  tags = {
    Name = "${var.app_name}-disk-high-alarm"
  }
}

# CloudWatch ダッシュボード
resource "aws_cloudwatch_dashboard" "main" {
  dashboard_name = "${var.app_name}-dashboard"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "metric"
        x      = 0
        y      = 0
        width  = 12
        height = 6
        properties = {
          metrics = [
            ["AWS/EC2", "CPUUtilization", "InstanceId", local.instance_id]
          ]
          period = 300
          stat   = "Average"
          region = var.region
          title  = "CPU使用率"
        }
      },
      {
        type   = "metric"
        x      = 12
        y      = 0
        width  = 12
        height = 6
        properties = {
          metrics = [
            ["CWAgent", "mem_used_percent", "InstanceId", local.instance_id]
          ]
          period = 300
          stat   = "Average"
          region = var.region
          title  = "メモリ使用率"
        }
      },
      {
        type   = "metric"
        x      = 0
        y      = 6
        width  = 12
        height = 6
        properties = {
          metrics = [
            ["CWAgent", "disk_used_percent", "InstanceId", local.instance_id, "path", "/", "fstype", "xfs"]
          ]
          period = 300
          stat   = "Average"
          region = var.region
          title  = "ディスク使用率"
        }
      },
      {
        type   = "metric"
        x      = 12
        y      = 6
        width  = 12
        height = 6
        properties = {
          metrics = [
            ["AWS/EC2", "NetworkIn", "InstanceId", local.instance_id],
            ["AWS/EC2", "NetworkOut", "InstanceId", local.instance_id]
          ]
          period = 300
          stat   = "Sum"
          region = var.region
          title  = "ネットワーク通信量"
        }
      }
    ]
  })
}
