data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/network/vpc/terraform.tfstate"
    region = var.region
  }
}

data "terraform_remote_state" "security_group" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/compute/security-group/terraform.tfstate"
    region = var.region
  }
}
