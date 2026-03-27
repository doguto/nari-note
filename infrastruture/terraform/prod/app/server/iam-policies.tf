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
