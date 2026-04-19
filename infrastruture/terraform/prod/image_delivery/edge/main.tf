locals {
  storage_state = data.terraform_remote_state.storage
  cert_state    = data.terraform_remote_state.cert
}

# CloudFront OAC（Origin Access Control）
resource "aws_cloudfront_origin_access_control" "images" {
  name                              = "${var.app_name}-images-oac"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "images" {
  enabled     = true
  price_class = "PriceClass_200"
  comment     = "${var.app_name} images"
  aliases     = ["image.nari-note.com"]

  origin {
    domain_name              = local.storage_state.outputs.bucket_regional_domain_name
    origin_id                = "S3-${local.storage_state.outputs.bucket_name}"
    origin_access_control_id = aws_cloudfront_origin_access_control.images.id
  }

  default_cache_behavior {
    target_origin_id       = "S3-${local.storage_state.outputs.bucket_name}"
    viewer_protocol_policy = "https-only"
    allowed_methods        = ["GET", "HEAD"]
    cached_methods         = ["GET", "HEAD"]
    compress               = true

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    min_ttl     = 0
    default_ttl = 86400
    max_ttl     = 31536000
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn      = local.cert_state.outputs.certificate_arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2021"
  }

  tags = {
    Name = "${var.app_name}-images-cf"
  }
}

resource "aws_ssm_parameter" "cloudfront_domain" {
  name  = "/${var.app_name}/app/CloudFront/ImagesDomain"
  type  = "String"
  value = "image.nari-note.com"
}

# S3 バケットポリシー: CloudFront OAC のみ読み取り許可
resource "aws_s3_bucket_policy" "images" {
  bucket = local.storage_state.outputs.bucket_name
  policy = data.aws_iam_policy_document.cloudfront_read.json
}

data "aws_iam_policy_document" "cloudfront_read" {
  statement {
    effect    = "Allow"
    actions   = ["s3:GetObject"]
    resources = ["${local.storage_state.outputs.bucket_arn}/*"]

    principals {
      type        = "Service"
      identifiers = ["cloudfront.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "AWS:SourceArn"
      values   = [aws_cloudfront_distribution.images.arn]
    }
  }
}
