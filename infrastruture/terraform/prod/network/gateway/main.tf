locals {
  vpc_id            = data.terraform_remote_state.vpc.outputs.vpc_id
  public_subnet_ids = data.terraform_remote_state.vpc.outputs.public_subnet_ids
}

resource "aws_internet_gateway" "main" {
  vpc_id = local.vpc_id

  tags = {
    Name = "${var.app_name}-internet-gateway"
  }
}

resource "aws_route_table" "public" {
  vpc_id = local.vpc_id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = {
    Name = "${var.app_name}-public-route-table"
  }
}

resource "aws_route_table_association" "public" {
  count = length(local.public_subnet_ids)

  subnet_id      = local.public_subnet_ids[count.index]
  route_table_id = aws_route_table.public.id
}
