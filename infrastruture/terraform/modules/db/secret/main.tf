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

# PostgreSQL は app-server と同一 EC2 インスタンス上で稼働しており、外部ネットワークには公開していない
# (localhost 接続のみ許可) ため、host/name は静的な値として管理する
resource "aws_ssm_parameter" "db_host" {
  name  = "/${var.app_name}/db/host"
  type  = "SecureString"
  value = "127.0.0.1"
}

resource "aws_ssm_parameter" "db_name" {
  name  = "/${var.app_name}/db/name"
  type  = "SecureString"
  value = "nari_note"
}
