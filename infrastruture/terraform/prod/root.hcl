locals {
    aws_region   = "ap-northeast-1"
    app_name     = "nari-note"
    s3_bucket    = "nari-note-terraform"
    state_prefix = "nari-note-app"

    template_dir = "${get_parent_terragrunt_dir()}/terragrunt-template"

    env_name     = "prod"
}

remote_state {
    backend = "s3"

    generate = {
        path      = "backend.tf"
        if_exists = "overwrite"
    }

    config = {
        bucket = local.s3_bucket
        key    = "${local.env_name}/${path_relative_to_include()}/terraform.tfstate"
        region = local.aws_region
    }
}

generate "provider" {
    path      = "provider.tf"
    if_exists = "overwrite"
    contents  = templatefile("${local.template_dir}/provider.tf.tpl", {
        aws_region = local.aws_region
    })
}

generate "terraform" {
    path      = "terraform.tf"
    if_exists = "overwrite"
    contents  = templatefile("${local.template_dir}/terraform.tf.tpl", {
        terraform_version = "1.14.6"
        aws_provider_version = "6.31.0"
    })
}

generate "global-variables" {
    path      = "global-variables.tf"
    if_exists = "overwrite"
    contents  = templatefile("${local.template_dir}/global-variables.tf.tpl", {
        aws_region = local.aws_region
        env_name   = local.env_name
        app_name   = local.app_name
        s3_bucket  = local.s3_bucket
    })
}
