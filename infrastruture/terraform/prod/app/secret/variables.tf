variable "jwt_secret" {
  description = "JWT secret key for token signing (minimum 32 characters)"
  type        = string
  sensitive   = true
}
