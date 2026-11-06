output "bucket_name" {
  description = "Name of the DB backup S3 bucket"
  value       = aws_s3_bucket.db_backup.bucket
}

output "bucket_arn" {
  description = "ARN of the DB backup S3 bucket"
  value       = aws_s3_bucket.db_backup.arn
}
