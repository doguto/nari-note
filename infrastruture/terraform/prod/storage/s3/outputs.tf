output "bucket_name" {
  value = aws_s3_bucket.app.bucket
}

output "bucket_arn" {
  value = aws_s3_bucket.app.arn
}

output "bucket_domain_name" {
  value = aws_s3_bucket.app.bucket_domain_name
}
