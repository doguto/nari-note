resource "aws_ssm_parameter" "cloudflare_origin_cert" {
  name  = "/${var.app_name}/nginx/cloudflare-origin-cert"
  type  = "SecureString"
  value = var.cloudflare_origin_cert
}

resource "aws_ssm_parameter" "cloudflare_origin_key" {
  name  = "/${var.app_name}/nginx/cloudflare-origin-key"
  type  = "SecureString"
  value = var.cloudflare_origin_key
}

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

resource "aws_ssm_parameter" "resend_api_key" {
  name  = "/${var.app_name}/app/resend_api_token"
  type  = "SecureString"
  value = var.resend_api_key
}
