data "terraform_remote_state" "server" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/app/server/terraform.tfstate"
    region = var.region
  }
}
