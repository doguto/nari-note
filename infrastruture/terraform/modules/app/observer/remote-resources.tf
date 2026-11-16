data "terraform_remote_state" "server" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/app/server/terraform.tfstate"
    region = var.region
  }
}

data "terraform_remote_state" "logger" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/app/logger/terraform.tfstate"
    region = var.region
  }
}
