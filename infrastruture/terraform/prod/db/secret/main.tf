resource "aws_ssm_parameter" "db_username" {
  name  = "/${var.app_name}/db/username"
  type  = "SecureString"
  value = var.db_username
}

resource "aws_ssm_parameter" "db_password" {
  name  = "/${var.app_name}/db/password"
  type  = "SecureString"
  value = var.db_password
}

resource "aws_ssm_parameter" "db_port" {
  name  = "/${var.app_name}/db/port"
  type  = "String"
  value = var.db_port
}
