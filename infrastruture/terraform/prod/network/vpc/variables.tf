variable "availability_zones" {
  description = "List of availability zones to use for the resources."
  type        = list(string)
  default     = ["ap-northeast-1a", "ap-northeast-1c"]
}
