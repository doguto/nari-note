locals {
  vpc_id                       = data.terraform_remote_state.vpc.outputs.vpc_id
  public_subnet_ids            = data.terraform_remote_state.vpc.outputs.public_subnet_ids
  app_server_availability_zone = data.aws_subnet.app_server.availability_zone

  # Amazon Linux 2023 (minimal, x86_64)
  # NOTE: most_recent で動的に解決すると、AMI の公開のたびに apply でインスタンスが再作成されるうえ、ルートボリュームに収まらない別系統の AMI を選ぶことがあるため、IDで固定する
  app_server_ami_id = "ami-005c30c039bdcb2b7"
}

resource "aws_ebs_volume" "postgres_data" {
  availability_zone = local.app_server_availability_zone
  size              = var.postgres_data_volume_size
  type              = "gp3"

  tags = {
    Name = "${var.app_name}-postgres-data"
  }
}

resource "aws_instance" "app_server" {
  ami                    = local.app_server_ami_id
  instance_type          = var.instance_type
  subnet_id              = local.public_subnet_ids[0]
  vpc_security_group_ids = [aws_security_group.app_server.id]

  # SSH 接続のためのキーペア
  key_name = aws_key_pair.app_server.key_name

  # EC2 への IAM ロールの割り当て
  iam_instance_profile = aws_iam_instance_profile.app_server.name

  # AMI のデフォルト(2GiB)だと dnf キャッシュ等ですぐ埋まるため拡張
  # スワップファイル(1GiB)も置くため 8GiB にしている
  root_block_device {
    volume_size = 8
    volume_type = "gp3"
  }

  user_data = templatefile("${path.module}/userdata.sh", {
    service_file       = templatefile("${path.module}/nari-note-backend.service", { app_name = var.app_name })
    nginx_conf_file    = file("${path.module}/nari-note-backend.nginx.conf")
    cloudwatch_conf    = templatefile("${path.module}/amazon-cloudwatch-agent.json", { app_name = var.app_name })
    postgresql_conf    = file("${path.module}/postgresql.conf")
    pg_hba_conf        = file("${path.module}/pg_hba.conf")
    app_name           = var.app_name
    postgres_volume_id = replace(aws_ebs_volume.postgres_data.id, "-", "")
  })
  user_data_replace_on_change = true

  tags = {
    Name = "${var.app_name}-app-server"
  }
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
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

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

resource "aws_key_pair" "app_server" {
  key_name   = "${var.app_name}-app-server-key"
  public_key = file(var.public_key_path)
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
