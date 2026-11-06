# 値は各環境の root.hcl の inputs から渡される
variable "region" { # tflint-ignore: terraform_unused_declarations
  description = "AWS Region"
  type        = string
}

variable "env_name" { # tflint-ignore: terraform_unused_declarations
  description = "Deployment Environment"
  type        = string
}

variable "app_name" { # tflint-ignore: terraform_unused_declarations
  description = "Application Name"
  type        = string
}

variable "state_bucket_name" { # tflint-ignore: terraform_unused_declarations
  description = "S3 Bucket for Terraform State"
  type        = string
}
