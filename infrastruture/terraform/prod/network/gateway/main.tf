locals {
  vpc_id = data.terraform_remote_state.vpc.outputs.vpc_id
}

resource "aws_internet_gateway" "main" {
  vpc_id = local.vpc_id

  tags = {
    Name = "${var.app_name}-internet-gateway"
  }
}
