variable "force_destroy" {
  description = "Allow the bucket to be destroyed even if it still contains objects/versions"
  type        = bool
  default     = false
}
