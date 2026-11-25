variable "memory_size" {
  description = "Memory size in MB of the engine function. 1769 MB gives one vCPU, which matches the engine's Threads=1"
  type        = number
  default     = 1769
}

variable "timeout" {
  description = "Timeout in seconds of the engine function. A shape judge takes tens of seconds"
  type        = number
  default     = 300
}

variable "reserved_concurrency" {
  description = "Upper limit of concurrent executions to cap the cost. null leaves it unreserved (the account limit applies)"
  type        = number
  default     = null
}

variable "log_retention_days" {
  description = "Number of days to retain the function logs"
  type        = number
  default     = 7
}
