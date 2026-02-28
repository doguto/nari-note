resource "aws_vpc" "nari-note-vpc" {
  # 最初の16bitをネットワーク部として固定する
  # 10.0.0.0 ~ 10.0.255.255 の範囲を通信で使用可能
  cidr_block = "10.0.0.0/16"

  tags = {
    Name = "${var.app_name}-vpc"
  }
}

# コストの削減のため、AZは2つのみに分割する
resource "aws_subnet" "nari-note-public-subnet" {
  count = 2

  vpc_id                  = aws_vpc.nari-note-vpc.id
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  # 最初の24bitをネットワーク部として固定する
  # 10.0.i.0 ~ 10.0.i.255 の範囲をサブネット内の通信で使用可能
  cidr_block = "10.0.${count.index}.0/24"

  tags = {
    Name = "${var.app_name}-public-subnet-${count.index}"
  }
}

resource "aws_subnet" "nari-note-private-subnet" {
  count = 2

  vpc_id                  = aws_vpc.nari-note-vpc.id
  map_public_ip_on_launch = false
  availability_zone       = var.availability_zones[count.index]

  # 最初の24bitをネットワーク部として固定する
  # 10.0.(2 + i).0 ~ 10.0.(2 + i).255 の範囲をサブネット内の通信で使用可能
  # public-subnetと重複しないように、iに2を足す
  cidr_block = "10.0.${2 + count.index}.0/24"

  tags = {
    Name = "${var.app_name}-private-subnet-${count.index}"
  }
}
