data "terraform_remote_state" "ec2" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/compute/ec2/terraform.tfstate"
    region = var.region
  }
}
