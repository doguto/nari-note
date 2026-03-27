data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/network/vpc/terraform.tfstate"
    region = var.region
  }
}

data "terraform_remote_state" "app_server" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/app/server/terraform.tfstate"
    region = var.region
  }
}

data "aws_ssm_parameter" "db_username" {
  name            = "/${var.app_name}/db/username"
  with_decryption = true
}

data "aws_ssm_parameter" "db_password" {
  name            = "/${var.app_name}/db/password"
  with_decryption = true
}
