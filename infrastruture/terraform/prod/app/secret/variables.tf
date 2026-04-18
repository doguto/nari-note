variable "cloudflare_origin_cert" {
  description = "Cloudflare Origin Certificate (PEM format)"
  type        = string
  sensitive   = true
  default     = "TMP"
}

variable "cloudflare_origin_key" {
  description = "Cloudflare Origin Certificate private key (PEM format)"
  type        = string
  sensitive   = true
  default     = "TMP"
}

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

variable "resend_api_key" {
  description = "Resend API key for sending emails"
  type        = string
  sensitive   = true
}

variable "sentry_dsn" {
  description = "Sentry DSN for error tracking"
  type        = string
  sensitive   = true
}
