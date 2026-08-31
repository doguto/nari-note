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
