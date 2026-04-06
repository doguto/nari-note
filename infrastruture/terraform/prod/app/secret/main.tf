resource "aws_ssm_parameter" "jwt_secret" {
  name  = "/${var.app_name}/app/Jwt/Secret"
  type  = "SecureString"
  value = var.jwt_secret
}
