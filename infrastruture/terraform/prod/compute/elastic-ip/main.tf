locals {
  instance_id = data.terraform_remote_state.ec2.outputs.instance_id
}

resource "aws_eip" "api" {
  instance = local.instance_id
  domain   = "vpc"

  tags = {
    Name = "${var.app_name}-api-eip"
  }
}
