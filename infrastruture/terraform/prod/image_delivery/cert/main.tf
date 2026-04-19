variable "domain_name" {
  description = "Custom domain for image delivery"
  type        = string
  default     = "image.nari-note.com"
}

resource "aws_acm_certificate" "images" {
  provider          = aws.us_east_1
  domain_name       = var.domain_name
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name = "${var.app_name}-images-cert"
  }
}

# apply 実行中に Cloudflare へ CNAME を追加するまで待機する
resource "aws_acm_certificate_validation" "images" {
  provider        = aws.us_east_1
  certificate_arn = aws_acm_certificate.images.arn

  timeouts {
    create = "10m"
  }
}
