output "public_ip" {
  value       = aws_eip.api.public_ip
  description = "The public IP address of the EC2 instance (api.nari-note.com)"
}

output "allocation_id" {
  value = aws_eip.api.allocation_id
}
