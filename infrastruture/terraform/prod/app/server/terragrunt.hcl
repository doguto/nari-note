include "root" {
  path = find_in_parent_folders("root.hcl")
}

dependencies {
  paths = [
    "../../network/vpc",
    "../../network/gateway",
    "../../deploy/bucket",
    "../../db/backup",
    "../secret",
    "../../db/secret",
    "../../image_delivery/storage"
  ]
}
