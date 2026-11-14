# CloudWatch Logs の保持期間を過ぎたログを残すため、前日分 (UTC) を毎日 S3 にエクスポートする

locals {
  log_archive_bucket_name = "${var.app_name}-logs"
  log_export_name         = "${var.app_name}-log-export"
  account_id              = data.aws_caller_identity.current.account_id

  archived_log_groups = [
    aws_cloudwatch_log_group.nginx_access,
    aws_cloudwatch_log_group.nginx_error,
    aws_cloudwatch_log_group.app,
    aws_cloudwatch_log_group.postgresql,
  ]
}

resource "aws_s3_bucket" "log_archive" {
  bucket = local.log_archive_bucket_name

  tags = {
    Name = local.log_archive_bucket_name
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "log_archive" {
  bucket = aws_s3_bucket.log_archive.id

  rule {
    blocked_encryption_types = ["SSE-C"]

    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "log_archive" {
  bucket = aws_s3_bucket.log_archive.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_lifecycle_configuration" "log_archive" {
  bucket = aws_s3_bucket.log_archive.id

  rule {
    id     = "archive-and-expire"
    status = "Enabled"

    filter {}

    transition {
      days          = var.log_archive_transition_days
      storage_class = "GLACIER_IR"
    }

    expiration {
      days = var.log_archive_retention_days
    }

    abort_incomplete_multipart_upload {
      days_after_initiation = 1
    }
  }
}

resource "aws_s3_bucket_policy" "log_archive" {
  bucket = aws_s3_bucket.log_archive.id
  policy = data.aws_iam_policy_document.log_archive_bucket.json

  depends_on = [aws_s3_bucket_public_access_block.log_archive]
}

data "aws_iam_policy_document" "log_archive_bucket" {
  statement {
    sid       = "CloudWatchLogsGetBucketAcl"
    actions   = ["s3:GetBucketAcl"]
    resources = [aws_s3_bucket.log_archive.arn]

    principals {
      type        = "Service"
      identifiers = ["logs.${var.region}.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "aws:SourceAccount"
      values   = [local.account_id]
    }

    condition {
      test     = "ArnLike"
      variable = "aws:SourceArn"
      values   = ["arn:aws:logs:${var.region}:${local.account_id}:log-group:*"]
    }
  }

  statement {
    sid       = "CloudWatchLogsPutObject"
    actions   = ["s3:PutObject"]
    resources = ["${aws_s3_bucket.log_archive.arn}/*"]

    principals {
      type        = "Service"
      identifiers = ["logs.${var.region}.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "s3:x-amz-acl"
      values   = ["bucket-owner-full-control"]
    }

    condition {
      test     = "StringEquals"
      variable = "aws:SourceAccount"
      values   = [local.account_id]
    }

    condition {
      test     = "ArnLike"
      variable = "aws:SourceArn"
      values   = ["arn:aws:logs:${var.region}:${local.account_id}:log-group:*"]
    }
  }
}

resource "aws_lambda_function" "log_export" {
  function_name    = local.log_export_name
  role             = aws_iam_role.log_export.arn
  runtime          = "python3.13"
  handler          = "index.handler"
  filename         = data.archive_file.log_export.output_path
  source_code_hash = data.archive_file.log_export.output_base64sha256
  timeout          = 900
  memory_size      = 128

  environment {
    variables = {
      BUCKET_NAME     = aws_s3_bucket.log_archive.id
      LOG_GROUP_NAMES = join(",", [for group in local.archived_log_groups : group.name])
    }
  }

  logging_config {
    log_format = "Text"
    log_group  = aws_cloudwatch_log_group.log_export.name
  }

  tags = {
    Name = local.log_export_name
  }
}

data "archive_file" "log_export" {
  type        = "zip"
  source_file = "${path.module}/log-export/index.py"
  output_path = "${path.module}/.build/log-export.zip"
}

resource "aws_cloudwatch_log_group" "log_export" {
  name              = "/aws/lambda/${local.log_export_name}"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${local.log_export_name}-log-group"
  }
}

resource "aws_iam_role" "log_export" {
  name               = "${local.log_export_name}-role"
  assume_role_policy = data.aws_iam_policy_document.log_export_assume_role.json
}

data "aws_iam_policy_document" "log_export_assume_role" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role_policy" "log_export" {
  name   = "${local.log_export_name}-policy"
  role   = aws_iam_role.log_export.id
  policy = data.aws_iam_policy_document.log_export.json
}

data "aws_iam_policy_document" "log_export" {
  statement {
    actions   = ["logs:CreateExportTask"]
    resources = flatten([for group in local.archived_log_groups : [group.arn, "${group.arn}:*"]])
  }

  statement {
    actions   = ["logs:DescribeExportTasks"]
    resources = ["*"]
  }

  # エクスポートタスクの作成者にもバケットへの書き込み権限が必要
  statement {
    actions   = ["s3:PutObject"]
    resources = ["${aws_s3_bucket.log_archive.arn}/*"]
  }

  statement {
    actions   = ["s3:ListBucket"]
    resources = [aws_s3_bucket.log_archive.arn]
  }

  statement {
    actions   = ["logs:CreateLogStream", "logs:PutLogEvents"]
    resources = ["${aws_cloudwatch_log_group.log_export.arn}:*"]
  }
}

resource "aws_scheduler_schedule" "log_export" {
  name                         = local.log_export_name
  schedule_expression          = "cron(30 0 * * ? *)"
  schedule_expression_timezone = "UTC"

  flexible_time_window {
    mode = "OFF"
  }

  target {
    arn      = aws_lambda_function.log_export.arn
    role_arn = aws_iam_role.log_export_scheduler.arn
  }
}

resource "aws_iam_role" "log_export_scheduler" {
  name               = "${local.log_export_name}-scheduler-role"
  assume_role_policy = data.aws_iam_policy_document.log_export_scheduler_assume_role.json
}

data "aws_iam_policy_document" "log_export_scheduler_assume_role" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["scheduler.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "aws:SourceAccount"
      values   = [local.account_id]
    }
  }
}

resource "aws_iam_role_policy" "log_export_scheduler" {
  name   = "${local.log_export_name}-scheduler-policy"
  role   = aws_iam_role.log_export_scheduler.id
  policy = data.aws_iam_policy_document.log_export_scheduler.json
}

data "aws_iam_policy_document" "log_export_scheduler" {
  statement {
    actions   = ["lambda:InvokeFunction"]
    resources = [aws_lambda_function.log_export.arn]
  }
}
