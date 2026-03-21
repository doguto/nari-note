output "instance_id" {
  value = aws_instance.api.id
}

output "instance_arn" {
  value = aws_instance.api.arn
}

output "security_group_id" {
  value = aws_security_group.ec2.id
}

output "public_ip" {
  value       = aws_eip.api.public_ip
  description = "The public IP address of the EC2 instance (api.nari-note.com)"
}

output "allocation_id" {
  value = aws_eip.api.allocation_id
}
