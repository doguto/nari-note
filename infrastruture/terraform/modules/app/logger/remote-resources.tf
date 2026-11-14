data "terraform_remote_state" "server" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/app/server/terraform.tfstate"
    region = var.region
  }
}

data "aws_caller_identity" "current" {}
