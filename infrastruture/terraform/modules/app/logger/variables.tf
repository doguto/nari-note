variable "log_retention_days" {
  description = "Number of days to retain CloudWatch logs"
  type        = number
  default     = 7
}

variable "log_archive_transition_days" {
  description = "Number of days before archived logs in S3 are moved to Glacier Instant Retrieval"
  type        = number
  default     = 90
}

variable "log_archive_retention_days" {
  description = "Number of days to retain archived logs in S3"
  type        = number
  default     = 365
}
