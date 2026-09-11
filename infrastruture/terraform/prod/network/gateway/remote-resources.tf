data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/network/vpc/terraform.tfstate"
    region = var.region
  }
}
