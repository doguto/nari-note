locals {
  instance_id  = data.terraform_remote_state.server.outputs.instance_id
  instance_arn = data.terraform_remote_state.server.outputs.instance_arn
}

# APIアプリケーションのロググループ
resource "aws_cloudwatch_log_group" "api" {
  name              = "/nari-note/ec2"
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

# アプリケーション用S3バケット（将棋記事の画像・ファイル保存）
resource "aws_s3_bucket" "app" {
  bucket = "${var.app_name}-app-${var.env_name}"

  tags = {
    Name = "${var.app_name}-app-bucket"
  }
}

# パブリックアクセスのブロック
resource "aws_s3_bucket_public_access_block" "app" {
  bucket = aws_s3_bucket.app.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# バケットのバージョニング有効化
resource "aws_s3_bucket_versioning" "app" {
  bucket = aws_s3_bucket.app.id

  versioning_configuration {
    status = "Enabled"
  }
}

# サーバーサイド暗号化
resource "aws_s3_bucket_server_side_encryption_configuration" "app" {
  bucket = aws_s3_bucket.app.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

# ライフサイクルルール（コスト最適化）
resource "aws_s3_bucket_lifecycle_configuration" "app" {
  bucket = aws_s3_bucket.app.id

  rule {
    id     = "transition-to-ia"
    status = "Enabled"

    transition {
      days          = 90
      storage_class = "STANDARD_IA"
    }

    noncurrent_version_expiration {
      noncurrent_days = 30
    }
  }
}

# EC2からのアクセスポリシー（IAMロール経由）
resource "aws_s3_bucket_policy" "app" {
  bucket = aws_s3_bucket.app.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowEC2Access"
        Effect = "Allow"
        Principal = {
          AWS = local.instance_arn
        }
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ]
        Resource = [
          aws_s3_bucket.app.arn,
          "${aws_s3_bucket.app.arn}/*"
        ]
      }
    ]
  })

  depends_on = [aws_s3_bucket_public_access_block.app]
}
