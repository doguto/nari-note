include "root" {
  path = find_in_parent_folders("root.hcl")
}

terraform {
  source = "${get_repo_root()}/infrastruture/terraform/prod/network/vpc"
}

inputs = {
  # 使い捨てのdev環境のため、AZを1つに絞って軽量化する
  availability_zones = ["ap-northeast-1a"]
}
