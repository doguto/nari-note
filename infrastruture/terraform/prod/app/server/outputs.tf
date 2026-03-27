output "instance_id" {
  value = aws_instance.app_server.id
}

output "security_group_id" {
  description = "Security group ID of the app server"
  value       = aws_security_group.app_server.id
}

output "public_ip" {
  description = "Elastic IP address (use this for Cloudflare DNS)"
  value       = aws_eip.app_server.public_ip
}
