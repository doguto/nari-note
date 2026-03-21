variable "ssh_allowed_cidr_blocks" {
  description = "List of CIDR blocks allowed to SSH into the EC2 instance"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}
