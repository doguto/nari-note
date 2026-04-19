data "terraform_remote_state" "storage" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "prod/image_delivery/storage/terraform.tfstate"
    region = var.region
  }
}
