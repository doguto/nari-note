locals {
  vpc_id            = data.terraform_remote_state.vpc.outputs.vpc_id
  public_subnet_ids = data.terraform_remote_state.vpc.outputs.public_subnet_ids
}

resource "aws_instance" "app_server" {
  ami                    = data.aws_ami.amazon_linux_2023.id
  instance_type          = var.instance_type
  subnet_id              = local.public_subnet_ids[0]
  vpc_security_group_ids = [aws_security_group.app_server.id]

  # SSH 接続のためのキーペア
  key_name = aws_key_pair.app_server.key_name

  # EC2 への IAM ロールの割り当て
  iam_instance_profile = aws_iam_instance_profile.app_server.name

  tags = {
    Name = "${var.app_name}-app-server"
  }
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
