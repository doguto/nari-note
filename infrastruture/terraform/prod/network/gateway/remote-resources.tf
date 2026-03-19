data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/network/vpc/terraform.tfstate"
    region = var.region
  }
}
