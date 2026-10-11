data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/network/vpc/terraform.tfstate"
    region = var.region
  }
}

data "terraform_remote_state" "deploy_bucket" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/deploy/bucket/terraform.tfstate"
    region = var.region
  }
}

data "terraform_remote_state" "images_storage" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/image_delivery/storage/terraform.tfstate"
    region = var.region
  }
}

data "aws_subnet" "app_server" {
  id = local.public_subnet_ids[0]
}
