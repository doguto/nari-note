resource "aws_s3_bucket" "backend" {
  bucket = "nari-note-terraform"

  # 基本的に使わないときは destroy するので、削除を有効化
  force_destroy = true
}

resource "aws_s3_bucket_versioning" "backend_versioning" {
  bucket = aws_s3_bucket.backend.id

  versioning_configuration {
    mfa_delete = "Disabled"
    status = "Enabled"
  }
}
