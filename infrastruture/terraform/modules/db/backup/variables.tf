variable "force_destroy" {
  description = "Allow the bucket to be destroyed even if it still contains objects/versions"
  type        = bool
  default     = false
}

variable "object_lock_days" {
  description = "Days each object version is protected from deletion by S3 Object Lock (GOVERNANCE). Must not exceed retention_days"
  type        = number
  default     = 7

  validation {
    condition     = var.object_lock_days >= 1 && var.object_lock_days <= var.retention_days
    error_message = "object_lock_days must be between 1 and retention_days."
  }
}

variable "retention_days" {
  description = "Days to keep WAL-G objects. PITR window is roughly this value minus one day"
  type        = number
  default     = 8
}
