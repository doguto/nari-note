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
}
