locals {
  public_subnet_ids  = data.terraform_remote_state.vpc.outputs.public_subnet_ids
  security_group_id  = data.terraform_remote_state.security_group.outputs.security_group_id
}

# Amazon Linux 2023の最新AMIを取得
data "aws_ami" "amazon_linux_2023" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-*-x86_64"]
  }

  filter {
    name   = "state"
    values = ["available"]
  }
}

resource "aws_instance" "api" {
  ami                    = data.aws_ami.amazon_linux_2023.id
  instance_type          = var.instance_type
  subnet_id              = local.public_subnet_ids[0]
  vpc_security_group_ids = [local.security_group_id]
  key_name               = var.key_name

  root_block_device {
    volume_type           = "gp3"
    volume_size           = 20
    delete_on_termination = true
    encrypted             = true

    tags = {
      Name = "${var.app_name}-root-volume"
    }
  }

  # CloudWatch エージェントのインストールとASP.NET Core環境のセットアップ
  user_data = <<-EOF
    #!/bin/bash
    set -e

    # システムの更新
    dnf update -y

    # .NET 9 SDKのインストール
    dnf install -y dotnet-sdk-9.0

    # CloudWatch エージェントのインストール
    dnf install -y amazon-cloudwatch-agent

    # CloudWatch エージェントの設定ファイル作成
    cat > /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json << 'CWCONFIG'
    {
      "logs": {
        "logs_collected": {
          "files": {
            "collect_list": [
              {
                "file_path": "/var/log/nari-note/app.log",
                "log_group_name": "/nari-note/api",
                "log_stream_name": "{instance_id}/app",
                "timestamp_format": "%Y-%m-%dT%H:%M:%S"
              }
            ]
          }
        }
      },
      "metrics": {
        "metrics_collected": {
          "mem": {
            "measurement": ["mem_used_percent"]
          },
          "disk": {
            "measurement": ["used_percent"],
            "resources": ["/"]
          }
        }
      }
    }
    CWCONFIG

    # ログディレクトリ作成
    mkdir -p /var/log/nari-note

    # CloudWatch エージェントの起動
    systemctl enable amazon-cloudwatch-agent
    systemctl start amazon-cloudwatch-agent
  EOF

  iam_instance_profile = aws_iam_instance_profile.ec2.name

  tags = {
    Name = "${var.app_name}-api-server"
  }
}

# CloudWatch エージェント用IAMロール
resource "aws_iam_role" "ec2" {
  name = "${var.app_name}-ec2-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name = "${var.app_name}-ec2-role"
  }
}

resource "aws_iam_role_policy_attachment" "cloudwatch_agent" {
  role       = aws_iam_role.ec2.name
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy"
}

resource "aws_iam_role_policy_attachment" "ssm_core" {
  role       = aws_iam_role.ec2.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_instance_profile" "ec2" {
  name = "${var.app_name}-ec2-instance-profile"
  role = aws_iam_role.ec2.name
}
