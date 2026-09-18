variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.micro"
}

variable "public_key_path" {
  description = "Path to the SSH public key file"
  type        = string
  default     = "~/.ssh/nari-note.pub"
}

variable "postgres_data_volume_size" {
  description = "Size in GB of the EBS volume used for the PostgreSQL data directory"
  type        = number
  default     = 20
}
