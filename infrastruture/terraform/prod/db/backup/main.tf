locals {
  bucket_name = "${var.app_name}-db-backup"
}

# PostgreSQL の WAL アーカイブ・ベースバックアップ (WAL-G) の保管先
resource "aws_s3_bucket" "db_backup" {
  bucket        = local.bucket_name
  force_destroy = var.force_destroy

  tags = {
    Name = local.bucket_name
  }
}

# EC2 には削除権限を与えていないが、上書きされた場合にも戻せるようにする
resource "aws_s3_bucket_versioning" "db_backup" {
  bucket = aws_s3_bucket.db_backup.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "db_backup" {
  bucket = aws_s3_bucket.db_backup.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "db_backup" {
  bucket = aws_s3_bucket.db_backup.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# 世代の削除は WAL-G ではなくライフサイクルに任せる (EC2 に s3:DeleteObject を与えないため)
# ベースバックアップは日次で取得するため、全オブジェクトを同じ日数で失効させても
# 残っている最古のベースバックアップ以降の WAL は必ず揃っている
resource "aws_s3_bucket_lifecycle_configuration" "db_backup" {
  bucket = aws_s3_bucket.db_backup.id

  rule {
    id     = "expire-wal-g"
    status = "Enabled"

    filter {
      prefix = "wal-g/"
    }

    expiration {
      days = var.retention_days
    }

    noncurrent_version_expiration {
      noncurrent_days = 7
    }
  }

  rule {
    id     = "cleanup-delete-markers"
    status = "Enabled"

    filter {}

    expiration {
      expired_object_delete_marker = true
    }

    abort_incomplete_multipart_upload {
      days_after_initiation = 1
    }
  }

  depends_on = [aws_s3_bucket_versioning.db_backup]
}
