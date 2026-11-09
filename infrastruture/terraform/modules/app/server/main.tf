locals {
  vpc_id                       = data.terraform_remote_state.vpc.outputs.vpc_id
  public_subnet_ids            = data.terraform_remote_state.vpc.outputs.public_subnet_ids
  app_server_availability_zone = data.aws_subnet.app_server.availability_zone
  deploy_bucket_name           = data.terraform_remote_state.deploy_bucket.outputs.bucket_name
  db_backup_bucket_name        = data.terraform_remote_state.db_backup.outputs.bucket_name
  db_backup_bucket_arn         = data.terraform_remote_state.db_backup.outputs.bucket_arn

  # Blue/Green デプロイスクリプトの S3 キー。userdata の上限 (16KB) を超えないよう、埋め込まずに S3 経由で配布する
  deploy_script_key = "scripts/nari-note-deploy.sh"
  # DB バックアップ (WAL-G) の導入・リストア用スクリプト。配布方法はデプロイスクリプトと同じ
  db_backup_script_key = "scripts/nari-note-db-backup.sh"

  # userdata が配置する設定ファイル。userdata の上限 (16KB) を超えないよう、埋め込まずに S3 経由で配布する
  server_config_files = {
    "nari-note-backend.nginx.conf" = file("${path.module}/nari-note-backend.nginx.conf")
    "nari-note-backend@.service"   = templatefile("${path.module}/nari-note-backend@.service", { app_name = var.app_name })
    "amazon-cloudwatch-agent.json" = templatefile("${path.module}/amazon-cloudwatch-agent.json", { app_name = var.app_name })
    "postgresql.conf"              = file("${path.module}/postgresql.conf")
    "pg_hba.conf"                  = file("${path.module}/pg_hba.conf")
    "dnf-automatic.conf"           = file("${path.module}/dnf-automatic.conf")
    "dnf-automatic.timer.conf"     = file("${path.module}/dnf-automatic.timer.conf")
    "dnf-automatic.service.conf"   = file("${path.module}/dnf-automatic.service.conf")
  }
  # 内容のハッシュをキーに含め、設定ファイルの変更で userdata が変わり EC2 が再作成されるようにする (起動時は userdata と同じ版を必ず取得する)
  server_config_prefix = "server-config/${md5(jsonencode(local.server_config_files))}"

  # Amazon Linux 2023 (standard, arm64)
  app_server_ami_id = "ami-0726b213222e0072c"
}

resource "aws_ebs_volume" "postgres_data" {
  availability_zone = local.app_server_availability_zone
  size              = var.postgres_data_volume_size
  type              = "gp3"

  tags = {
    Name = "${var.app_name}-postgres-data"
    # DLM のスナップショット対象を識別するタグ
    Snapshot = "${var.app_name}-postgres-data"
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_instance" "app_server" {
  ami                    = local.app_server_ami_id
  instance_type          = var.instance_type
  subnet_id              = local.public_subnet_ids[0]
  vpc_security_group_ids = [aws_security_group.app_server.id]

  # EC2 への IAM ロールの割り当て
  iam_instance_profile = aws_iam_instance_profile.app_server.name

  # dnf キャッシュとスワップファイル(1GiB)を置くため 8GiB にしている
  root_block_device {
    volume_size = 8
    volume_type = "gp3"
  }

  user_data = templatefile("${path.module}/userdata.sh", {
    server_config_prefix  = local.server_config_prefix
    app_name              = var.app_name
    postgres_volume_id    = replace(aws_ebs_volume.postgres_data.id, "-", "")
    deploy_script_key     = aws_s3_object.deploy_script.key
    db_backup_script_key  = aws_s3_object.db_backup_script.key
    db_backup_bucket_name = local.db_backup_bucket_name
  })
  user_data_replace_on_change = true

  # userdata は prefix を文字列で受け取るだけなので、起動前に設定ファイルが S3 にあるよう明示する
  depends_on = [aws_s3_object.server_config]

  tags = {
    Name = "${var.app_name}-app-server"
  }
}

# 初回起動時に userdata が取得する設定ファイル
resource "aws_s3_object" "server_config" {
  for_each = local.server_config_files

  bucket  = local.deploy_bucket_name
  key     = "${local.server_config_prefix}/${each.key}"
  content = each.value
  etag    = md5(each.value)
}

# 初回起動時に userdata が取得する Blue/Green デプロイスクリプト
resource "aws_s3_object" "deploy_script" {
  bucket = local.deploy_bucket_name
  key    = local.deploy_script_key
  source = "${path.module}/nari-note-deploy.sh"
  etag   = filemd5("${path.module}/nari-note-deploy.sh")
}

# 初回起動時に userdata が取得する DB バックアップスクリプト
resource "aws_s3_object" "db_backup_script" {
  bucket = local.deploy_bucket_name
  key    = local.db_backup_script_key
  source = "${path.module}/nari-note-db-backup.sh"
  etag   = filemd5("${path.module}/nari-note-db-backup.sh")
}

resource "aws_volume_attachment" "postgres_data" {
  device_name = "/dev/sdf"
  volume_id   = aws_ebs_volume.postgres_data.id
  instance_id = aws_instance.app_server.id
}

resource "aws_security_group" "app_server" {
  name        = "${var.app_name}-app-server-sg"
  description = "Security group for app server"
  vpc_id      = local.vpc_id

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.app_name}-app-server-sg"
  }
}

resource "aws_eip" "app_server" {
  instance = aws_instance.app_server.id
  domain   = "vpc"

  tags = {
    Name = "${var.app_name}-app-server-eip"
  }
}

resource "aws_iam_instance_profile" "app_server" {
  name = "${var.app_name}-app-server-profile"
  role = aws_iam_role.app_server.name
}

resource "aws_iam_role" "app_server" {
  name               = "${var.app_name}-app-server-role"
  assume_role_policy = data.aws_iam_policy_document.app_server_assume_role.json

  tags = {
    Name = "${var.app_name}-app-server-role"
  }
}

data "aws_iam_policy_document" "app_server_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ec2.amazonaws.com"]
    }
  }
}

# PostgreSQL データボリュームの定期スナップショット
# WAL も同一ボリュームにあるため、クラッシュ整合のスナップショットでも復元時に WAL リカバリで整合状態に戻る
resource "aws_dlm_lifecycle_policy" "postgres_data" {
  description        = "${var.app_name} postgres data snapshot"
  execution_role_arn = aws_iam_role.dlm.arn
  state              = "ENABLED"

  policy_details {
    resource_types = ["VOLUME"]
    target_tags = {
      Snapshot = aws_ebs_volume.postgres_data.tags["Snapshot"]
    }

    schedule {
      name = "daily"
      create_rule {
        interval      = 24
        interval_unit = "HOURS"
        times         = ["18:00"] # JST 03:00
      }
      retain_rule {
        count = 7
      }
      copy_tags = true
    }

    schedule {
      name = "weekly"
      create_rule {
        cron_expression = "cron(30 18 ? * SUN *)" # JST 月曜 03:30
      }
      retain_rule {
        count = 4
      }
      copy_tags = true
    }
  }

  tags = {
    Name = "${var.app_name}-postgres-data-snapshot"
  }
}
