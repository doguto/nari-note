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
