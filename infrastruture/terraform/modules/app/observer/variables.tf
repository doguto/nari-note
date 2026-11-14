variable "alert_email" {
  description = "The email address to receive CloudWatch alarm notifications."
  type        = string
  sensitive   = true
}

variable "cpu_alarm_threshold" {
  description = "CPU utilization percentage that triggers the alarm."
  type        = number
  default     = 80
}

variable "cpu_alarm_evaluation_periods" {
  description = "Number of consecutive periods the threshold must be breached before alarming."
  type        = number
  default     = 1
}

variable "cpu_alarm_period" {
  description = "Length of each evaluation period, in seconds."
  type        = number
  default     = 300
}

variable "wal_archive_pending_threshold" {
  description = "Number of WAL segments waiting to be archived that triggers the alarm. With archive_timeout = 60s, this is roughly the backlog in minutes."
  type        = number
  default     = 5
}

variable "db_error_log_threshold" {
  description = "Number of PostgreSQL ERROR/FATAL/PANIC log entries within 5 minutes that triggers the alarm."
  type        = number
  default     = 5
}

variable "memory_used_percent_threshold" {
  description = "Memory usage percentage (5-minute average) that triggers the alarm."
  type        = number
  default     = 90
}

variable "swap_used_percent_threshold" {
  description = "Swap usage percentage (5-minute average) that triggers the alarm."
  type        = number
  default     = 50
}

variable "db_connections_used_percent_threshold" {
  description = "Percentage of max_connections in use by client backends that triggers the alarm."
  type        = number
  default     = 80
}

variable "db_xid_age_threshold" {
  description = "Maximum age(datfrozenxid) across databases that triggers the alarm. Wraparound protection stops writes at about 2 billion."
  type        = number
  default     = 500000000
}

variable "disk_used_percent_threshold" {
  description = "Disk usage percentage of / and /data/postgresql that triggers the alarm."
  type        = number
  default     = 80
}
