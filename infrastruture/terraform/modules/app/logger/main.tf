locals {
  instance_id = data.terraform_remote_state.server.outputs.instance_id
}

# Nginx アクセスログ
resource "aws_cloudwatch_log_group" "nginx_access" {
  name              = "/${var.app_name}/nginx/access"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${var.app_name}-nginx-access-log-group"
  }
}

# Nginx エラーログ
resource "aws_cloudwatch_log_group" "nginx_error" {
  name              = "/${var.app_name}/nginx/error"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${var.app_name}-nginx-error-log-group"
  }
}

# ASP.NET アプリケーションログ（journald 経由）
resource "aws_cloudwatch_log_group" "app" {
  name              = "/${var.app_name}/application"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${var.app_name}-app-log-group"
  }
}

# PostgreSQL サーバーログ (遅延クエリ・ロック待ち・エラー)
resource "aws_cloudwatch_log_group" "postgresql" {
  name              = "/${var.app_name}/postgresql"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${var.app_name}-postgresql-log-group"
  }
}

# ERROR / FATAL / PANIC の件数をメトリクス化する。アラームは app/observer で定義する (namespace・metric 名を合わせること)
resource "aws_cloudwatch_log_metric_filter" "postgresql_error" {
  name           = "${var.app_name}-postgresql-error"
  log_group_name = aws_cloudwatch_log_group.postgresql.name
  pattern        = "?\"ERROR:\" ?\"FATAL:\" ?\"PANIC:\""

  metric_transformation {
    namespace     = "${var.app_name}/PostgreSQL"
    name          = "ErrorLogCount"
    value         = "1"
    default_value = "0"
  }
}
