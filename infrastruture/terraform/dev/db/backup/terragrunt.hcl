include "root" {
  path = find_in_parent_folders("root.hcl")
}

terraform {
  source = "${get_repo_root()}/infrastruture/terraform/prod/db/backup"
}

inputs = {
  # devは使い捨てのため、バージョンが残っていてもdestroyできるようにする
  force_destroy = true
}
