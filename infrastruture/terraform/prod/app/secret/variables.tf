variable "jwt_secret" {
  description = "JWT secret key for token signing (minimum 32 characters)"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer (e.g. https://example.com)"
  type        = string
}

variable "jwt_audience" {
  description = "JWT audience (e.g. https://example.com)"
  type        = string
}
