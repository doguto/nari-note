locals {
  instance_id = data.terraform_remote_state.server.outputs.instance_id
}

# TODO: Nginx アクセスログ (/${var.app_name}/nginx/access)
# TODO: Nginx エラーログ (/${var.app_name}/nginx/error)

# ASP.NET アプリケーションログ（journald 経由）
resource "aws_cloudwatch_log_group" "app" {
  name              = "/${var.app_name}/application"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${var.app_name}-app-log-group"
  }
}
