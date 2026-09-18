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

data "aws_ami" "amazon_linux_2023" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-*-x86_64"]
  }

  filter {
    name   = "architecture"
    values = ["x86_64"]
  }
}

data "aws_subnet" "app_server" {
  id = local.public_subnet_ids[0]
}
