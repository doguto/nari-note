output "bucket_name" {
  description = "Name of the deployment S3 bucket"
  value       = aws_s3_bucket.deploy.bucket
}

output "bucket_arn" {
  description = "ARN of the deployment S3 bucket"
  value       = aws_s3_bucket.deploy.arn
}
