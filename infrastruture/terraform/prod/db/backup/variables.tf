variable "force_destroy" {
  description = "Allow the bucket to be destroyed even if it still contains objects/versions"
  type        = bool
  default     = false
}

variable "retention_days" {
  description = "Days to keep WAL-G objects. PITR window is roughly this value minus one day"
  type        = number
  default     = 8
}
