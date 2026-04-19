output "certificate_arn" {
  description = "ARN of the ACM certificate (us-east-1)"
  value       = aws_acm_certificate.images.arn
}

output "domain_validation_options" {
  description = "CNAME records to add in Cloudflare for DNS validation"
  value       = aws_acm_certificate.images.domain_validation_options
}
