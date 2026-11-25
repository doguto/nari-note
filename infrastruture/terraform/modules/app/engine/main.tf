# マイ定跡の研究盤が使う将棋エンジン。zip は scripts/deploy-engine.sh が deploy バケットに置き、コードの更新もそのスクリプトが行う

locals {
  engine_name        = "${var.app_name}-shogi-engine"
  deploy_bucket_name = data.terraform_remote_state.deploy_bucket.outputs.bucket_name
  app_server_role    = data.terraform_remote_state.server.outputs.role_name
}

resource "aws_lambda_function" "engine" {
  function_name                  = local.engine_name
  role                           = aws_iam_role.engine.arn
  runtime                        = "python3.13"
  handler                        = "lambda_function.handler"
  architectures                  = ["x86_64"]
  s3_bucket                      = local.deploy_bucket_name
  s3_key                         = "engine/latest/nari-note-engine.zip"
  memory_size                    = var.memory_size
  timeout                        = var.timeout
  reserved_concurrent_executions = var.reserved_concurrency

  environment {
    variables = {
      ENGINE_PATH = "/var/task/engine/YaneuraOu-by-gcc"
    }
  }

  logging_config {
    log_format = "Text"
    log_group  = aws_cloudwatch_log_group.engine.name
  }

  tags = {
    Name = local.engine_name
  }
}

resource "aws_cloudwatch_log_group" "engine" {
  name              = "/aws/lambda/${local.engine_name}"
  retention_in_days = var.log_retention_days

  tags = {
    Name = "${local.engine_name}-log-group"
  }
}

resource "aws_iam_role" "engine" {
  name               = "${local.engine_name}-role"
  assume_role_policy = data.aws_iam_policy_document.engine_assume_role.json
}

data "aws_iam_policy_document" "engine_assume_role" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role_policy" "engine" {
  name   = "${local.engine_name}-policy"
  role   = aws_iam_role.engine.id
  policy = data.aws_iam_policy_document.engine.json
}

data "aws_iam_policy_document" "engine" {
  statement {
    actions   = ["logs:CreateLogStream", "logs:PutLogEvents"]
    resources = ["${aws_cloudwatch_log_group.engine.arn}:*"]
  }
}

# バックエンドは SSM の /<app_name>/app 以下を設定として読むので、ShogiEngine:FunctionName として渡る
resource "aws_ssm_parameter" "function_name" {
  name  = "/${var.app_name}/app/ShogiEngine/FunctionName"
  type  = "String"
  value = aws_lambda_function.engine.function_name
}

resource "aws_iam_role_policy" "app_server_invoke" {
  name   = "${local.engine_name}-invoke-policy"
  role   = local.app_server_role
  policy = data.aws_iam_policy_document.app_server_invoke.json
}

data "aws_iam_policy_document" "app_server_invoke" {
  statement {
    actions   = ["lambda:InvokeFunction"]
    resources = [aws_lambda_function.engine.arn]
  }
}
