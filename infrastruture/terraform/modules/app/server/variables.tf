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

variable "enable_db_health_check" {
  description = "Whether to send PostgreSQL health metrics (DbUp, DiskUsedPercent, DbWalArchivePendingFiles) to CloudWatch every 5 minutes. Disable where no alarms consume them."
  type        = bool
  default     = true
}
