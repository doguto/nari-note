variable "region" { # tflint-ignore: terraform_unused_declarations
  description = "AWS Region"
  type        = string
  default     = "${aws_region}"
}

variable "env_name" { # tflint-ignore: terraform_unused_declarations
  description = "Deployment Environment"
  type        = string
  default     = "${env_name}"
}

variable "app_name" { # tflint-ignore: terraform_unused_declarations
  description = "Application Name"
  type        = string
  default     = "${app_name}"
}

variable "state_bucket_name" { # tflint-ignore: terraform_unused_declarations
  description = "S3 Bucket for Terraform State"
  type        = string
  default     = "${s3_bucket}"
}
