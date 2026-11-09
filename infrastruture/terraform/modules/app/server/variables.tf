variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t4g.micro"
}

variable "postgres_data_volume_size" {
  description = "Size in GB of the EBS volume used for the PostgreSQL data directory"
  type        = number
  default     = 20
}
