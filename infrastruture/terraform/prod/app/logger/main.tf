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
