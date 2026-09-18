include "root" {
  path = find_in_parent_folders("root.hcl")
}

terraform {
  source = "${get_repo_root()}/infrastruture/terraform/prod/deploy/bucket"
}

inputs = {
  # devは使い捨てのため、バージョンが残っていてもdestroyできるようにする
  force_destroy = true
}

dependencies {
  paths = [
    "../../network/vpc"
  ]
}
