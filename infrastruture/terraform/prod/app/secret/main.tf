resource "aws_ssm_parameter" "jwt_secret" {
  name  = "/${var.app_name}/app/Jwt/Secret"
  type  = "SecureString"
  value = var.jwt_secret
}

resource "aws_ssm_parameter" "jwt_issuer" {
  name  = "/${var.app_name}/app/Jwt/Issuer"
  type  = "String"
  value = var.jwt_issuer
}

resource "aws_ssm_parameter" "jwt_audience" {
  name  = "/${var.app_name}/app/Jwt/Audience"
  type  = "String"
  value = var.jwt_audience
}
