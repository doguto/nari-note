variable "db_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "db_allocated_storage" {
  description = "Allocated storage size in GB"
  type        = number
  default     = 20
}

variable "db_storage_type" {
  description = "RDS storage type (gp2, gp3, io1)"
  type        = string

  # AWS推奨のgp3をデフォルトに設定
  default = "gp3"
}
