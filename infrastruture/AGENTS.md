# nari-note インフラ

nari-note アプリケーションの AWS インフラを Terraform + Terragrunt で管理するリポジトリ。

## ファイル構成

```
xxx/
  ├── backend.tf           # Terraform のバックエンド設定（terragrunt生成）
  ├── global-variables.tf  # 全体で使用する変数定義（terragrunt生成）
  ├── main.tf              # Terraform のリソース定義
  ├── outputs.tf           # Terraform の出力定義
  ├── provider.tf          # Terraform のプロバイダ設定 (terragrunt生成)
  ├── remote-resources.tf  # リモートリソース定義 (data ソースなど)
  ├── terraform.tf         # Terraform の設定ファイル（terragrunt生成）
  ├── terragrunt.hcl       # Terragrunt の設定ファイル
  └── variables.tf         # Terraform の変数定義
```

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
