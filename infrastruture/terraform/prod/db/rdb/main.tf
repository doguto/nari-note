locals {
  vpc_id             = data.terraform_remote_state.vpc.outputs.vpc_id
  private_subnet_ids = data.terraform_remote_state.vpc.outputs.private_subnet_ids
  app_server_sg_id   = data.terraform_remote_state.app_server.outputs.security_group_id

  db_username = data.aws_ssm_parameter.db_username.value
  db_password = data.aws_ssm_parameter.db_password.value
}

resource "aws_db_instance" "main" {
  identifier = "${var.app_name}-rdb"

  engine         = "postgres"
  engine_version = "17"
  instance_class = var.db_instance_class

  allocated_storage = var.db_allocated_storage
  storage_type      = var.db_storage_type
  storage_encrypted = true

  db_name = "nari_note"

  username = local.db_username
  password = local.db_password

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.rdb.id]

  # DBの設定パラメータ
  parameter_group_name = aws_db_parameter_group.main.name

  # コスト削減のため、マルチAZは無効化
  multi_az = false

  publicly_accessible = false

  # DBインスタンス削除時のスナップショット作成をスキップ
  # 本番環境では本来はスナップショットを取得してから削除するべきだが、今回はコスト削減のためスキップする
  skip_final_snapshot = true

  # JST 04:00-04:30 = UTC 19:00-19:30 (金→土)
  maintenance_window = "fri:19:00-fri:19:30"

  # JST 02:00-03:00 = UTC 17:00-18:00
  backup_window = "17:00-18:00"

  # バックアップは 設定したストレージ容量 (今回なら20GB) 以内であれば無料で利用可能
  # よって保持期間を短めの3日に設定し、ストレージが超過しないようにする
  # 本体が 20/3 = 約6.7GB 以上のデータを保持するようになったら、保持期間をさらに短くするか、ストレージ容量を増やす必要がある
  backup_retention_period = 3

  tags = {
    Name = "${var.app_name}-rdb"
  }
}

# https://docs.aws.amazon.com/ja_jp/AmazonRDS/latest/UserGuide/Appendix.PostgreSQL.CommonDBATasks.Parameters.html
resource "aws_db_parameter_group" "main" {
  name   = "${var.app_name}-rdb-pg"
  family = "postgres17"

  tags = {
    Name = "${var.app_name}-rdb-pg"
  }
}

resource "aws_db_subnet_group" "main" {
  name       = "${var.app_name}-db-subnet-group"
  subnet_ids = local.private_subnet_ids

  tags = {
    Name = "${var.app_name}-db-subnet-group"
  }
}

resource "aws_ssm_parameter" "db_host" {
  name  = "/${var.app_name}/db/host"
  type  = "SecureString"
  value = aws_db_instance.main.address

  tags = {
    Name = "${var.app_name}-db-host"
  }
}

resource "aws_ssm_parameter" "db_name" {
  name  = "/${var.app_name}/db/name"
  type  = "SecureString"
  value = aws_db_instance.main.db_name

  tags = {
    Name = "${var.app_name}-db-name"
  }
}

resource "aws_security_group" "rdb" {
  name        = "${var.app_name}-rdb-sg"
  description = "Security group for RDS PostgreSQL"
  vpc_id      = local.vpc_id

  ingress {
    description     = "PostgreSQL from app server"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [local.app_server_sg_id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.app_name}-rdb-sg"
  }
}
