variable "db_username" {
  description = "Master username for the RDS instance"
  type        = string

  sensitive = true
}

variable "db_password" {
  description = "Master password for the RDS instance"
  type        = string

  sensitive = true
}

variable "db_port" {
  description = "Port number for the RDS instance"
  type        = string
  default     = "5432"
}
