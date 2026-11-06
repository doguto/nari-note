# nari-note インフラ

nari-note アプリケーションの AWS インフラを Terraform + Terragrunt で管理するリポジトリ。

## ディレクトリ構成

```
terraform/
  ├── modules/              # リソース定義の実体（環境に依存しない）
  │   └── <group>/<unit>/
  ├── prod/                 # 本番環境
  │   ├── root.hcl          # 環境共通の設定（state / provider / 共通変数の生成）
  │   └── <group>/<unit>/
  │       └── terragrunt.hcl
  ├── dev/                  # 開発環境（構成は prod と同じ）
  └── terragrunt-template/  # root.hcl から生成するファイルのテンプレート
```

各環境の `<group>/<unit>/terragrunt.hcl` は `terraform.source` で `modules/<group>/<unit>` を参照し、環境ごとの差分は `inputs` で渡す。
秘匿値を含む `terraform.tfvars` は各環境のユニットディレクトリに置く（Git 管理外）。
state のキーは `<env_name>/<group>/<unit>/terraform.tfstate` となる。

### modules/<group>/<unit> のファイル構成

```
xxx/
  ├── global-variables.tf  # 全体で使用する変数定義（値は root.hcl の inputs から渡す）
  ├── main.tf              # Terraform のリソース定義
  ├── outputs.tf           # Terraform の出力定義
  ├── remote-resources.tf  # リモートリソース定義 (data ソースなど)
  └── variables.tf         # Terraform の変数定義
```

`global-variables.tf`（`region` / `env_name` / `app_name` / `state_bucket_name`）は全モジュールで同じ内容とし、default は設定しない。

以下は Terragrunt が `root.hcl` から実行時に生成するため、`modules/` には置かない。

- `backend.tf`: バックエンド設定
- `provider.tf`: プロバイダ設定
- `terraform.tf`: Terraform / プロバイダのバージョン指定

別ユニットの state を参照する際は、環境名をハードコードせず `${var.env_name}` を使うこと。

## main.tf のコーディング規約

以下のように `locals` → `resource` の順で記述すること。
`resource` はメインとなるリソースを上部に配置し、関連するリソースはその下に記述すること。

```hcl
locals {
  # 定数や共通の値を定義
}

# app/server なら、EC2インスタンスが一番メインなリソースになるため、最初に記述する。
resource "aws_instance" "example" {
  # リソース定義
}

# EC2インスタンスを作成するために必要なセキュリティグループなどの関連リソースは、EC2インスタンスの下に記述する。
resource "aws_security_group" "example_sg" {
  # 関連するリソース定義
}
```

## remote-resource のコーディング規約

リモートリソースは以下のように `remote-resources.tf` にまとめて記述すること。

```hcl
data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = "nari-note-terraform-state"
    key    = "network/vpc/terraform.tfstate"
    region = "ap-northeast-1"
  }
}

data "aws_ssm_parameter" "db_username" {
  name = "/prod/db/username"
}
```

`remote-resources.tf` に記述した後、実際に `main.tf` などで使用する際は以下のように `locals` に代入してから使用すること。

```hcl
locals {
  vpc_id      = data.terraform_remote_state.vpc.outputs.vpc_id
  db_username = data.aws_ssm_parameter.db_username.value
}
```
