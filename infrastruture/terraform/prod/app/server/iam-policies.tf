resource "aws_iam_role_policy_attachment" "ssm_managed_instance_core" {
  role       = aws_iam_role.app_server.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_role_policy" "ssm_read" {
  name   = "${var.app_name}-ssm-read-policy"
  role   = aws_iam_role.app_server.id
  policy = data.aws_iam_policy_document.ssm_read.json
}

resource "aws_iam_role_policy" "cloudwatch_agent" {
  name   = "${var.app_name}-cloudwatch-agent-policy"
  role   = aws_iam_role.app_server.id
  policy = data.aws_iam_policy_document.cloudwatch_agent.json
}

data "aws_iam_policy_document" "ssm_read" {
  statement {
    effect = "Allow"
    actions = [
      "ssm:GetParameter",
      "ssm:GetParametersByPath",
    ]
    resources = [
      "arn:aws:ssm:*:*:parameter/${var.app_name}/*",
    ]
  }

  statement {
    effect = "Allow"
    actions = [
      "kms:Decrypt",
    ]
    resources = ["*"]
  }
}

resource "aws_iam_role_policy" "s3_deploy_read" {
  name   = "${var.app_name}-s3-deploy-read-policy"
  role   = aws_iam_role.app_server.id
  policy = data.aws_iam_policy_document.s3_deploy_read.json
}

data "aws_iam_policy_document" "s3_deploy_read" {
  statement {
    effect = "Allow"
    actions = [
      "s3:GetObject",
      "s3:ListBucket",
    ]
    resources = [
      data.terraform_remote_state.deploy_bucket.outputs.bucket_arn,
      "${data.terraform_remote_state.deploy_bucket.outputs.bucket_arn}/*",
    ]
  }
}

data "aws_iam_policy_document" "cloudwatch_agent" {
  statement {
    effect = "Allow"
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents",
      "logs:DescribeLogGroups",
      "logs:DescribeLogStreams",
    ]
    resources = ["*"]
  }

  statement {
    effect    = "Allow"
    actions   = ["cloudwatch:PutMetricData"]
    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "cloudwatch:namespace"
      values   = ["CWAgent"]
    }
  }
}

resource "aws_iam_role_policy" "s3_images_write" {
  name   = "${var.app_name}-s3-images-write-policy"
  role   = aws_iam_role.app_server.id
  policy = data.aws_iam_policy_document.s3_images_write.json
}

data "aws_iam_policy_document" "s3_images_write" {
  statement {
    effect = "Allow"
    actions = [
      "s3:PutObject",
      "s3:DeleteObject",
    ]
    resources = [
      "${data.terraform_remote_state.images_storage.outputs.bucket_arn}/*",
    ]
  }
}

resource "aws_iam_role" "dlm" {
  name               = "${var.app_name}-dlm-role"
  assume_role_policy = data.aws_iam_policy_document.dlm_assume_role.json

  tags = {
    Name = "${var.app_name}-dlm-role"
  }
}

data "aws_iam_policy_document" "dlm_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["dlm.amazonaws.com"]
    }
  }
}

resource "aws_iam_role_policy_attachment" "dlm" {
  role       = aws_iam_role.dlm.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSDataLifecycleManagerServiceRole"
}

# WAL-G による WAL・ベースバックアップの保存と復元
# 乗っ取り・誤操作でバックアップを消されないよう、s3:DeleteObject は付与しない (世代削除はライフサイクルで行う)
resource "aws_iam_role_policy" "s3_db_backup" {
  name   = "${var.app_name}-s3-db-backup-policy"
  role   = aws_iam_role.app_server.id
  policy = data.aws_iam_policy_document.s3_db_backup.json
}

data "aws_iam_policy_document" "s3_db_backup" {
  statement {
    effect = "Allow"
    actions = [
      "s3:PutObject",
      "s3:GetObject",
    ]
    resources = ["${local.db_backup_bucket_arn}/*"]
  }

  statement {
    effect    = "Allow"
    actions   = ["s3:ListBucket"]
    resources = [local.db_backup_bucket_arn]
  }
}
