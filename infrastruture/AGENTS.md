# nari-note インフラ

nari-note の AWS インフラを Terraform + Terragrunt で管理する。
システム構成・デプロイの流れは [docs/infra-design.md](docs/infra-design.md)、環境構築は [docs/setup.md](docs/setup.md) を参照。

## 作業上の注意

- `apply` / `destroy` / `state` 操作・デプロイスクリプトの実行は、ユーザーの明示的な指示なしに行わない (本番に直結する)
- `terraform.tfvars` (秘匿値) と `terraform/backend/` 以外の tfstate は Git 管理外。読み書きしない
- ディレクトリ名 `infrastruture` は綴りが誤っているが、パスが各所で参照されているため改名しない

## ディレクトリ構成

```
terraform/
  ├── modules/<group>/<unit>/     # リソース定義の実体（環境に依存しない）
  ├── prod/                       # 本番環境
  │   ├── root.hcl                # 環境共通の設定（state / provider / 共通変数の生成）
  │   └── <group>/<unit>/terragrunt.hcl
  ├── dev/                        # 開発環境（prod のユニットの一部）
  ├── terragrunt-template/        # root.hcl が生成するファイルのテンプレート
  ├── backend/                    # state 用 S3 バケット（素の Terraform。tfstate は Git 管理）
  └── budget/                     # 月額予算アラート（素の Terraform）
```

- 環境の `terragrunt.hcl` は `include "root"` で `root.hcl` を読み、`terraform.source` で `modules/<group>/<unit>` を参照する。依存ユニットは `dependencies` に書く
- 環境ごとの差分は `inputs` または `terraform.tfvars`（ユニットディレクトリに置く。Git 管理外）で渡す
- state のキーは `<env_name>/<group>/<unit>/terraform.tfstate`

## モジュールのファイル構成

| ファイル | 内容 |
| --- | --- |
| `global-variables.tf` | `region` / `env_name` / `app_name` / `state_bucket_name`。全モジュールで同じ内容とし、default は設定しない（値は `root.hcl` の `inputs` から渡る） |
| `main.tf` | リソース定義 |
| `variables.tf` | モジュール固有の変数 |
| `outputs.tf` | 他ユニットから参照する出力 |
| `remote-resources.tf` | `data` ソース（他ユニットの state、既存リソース等） |

- 大きいモジュールは用途別に分けてよい（例: `app/server/iam-policies.tf`）。EC2 に配る設定ファイル・スクリプトもモジュール内に置く
- `backend.tf` / `provider.tf` / `terraform.tf` は Terragrunt が生成するため `modules/` に置かない。別リージョンが必要な場合のみ、alias 付きプロバイダを別ファイルで定義する（例: CloudFront 用 ACM の `image_delivery/cert/provider-acm.tf`）

## 命名

- リソース名・タグの `Name` には `${var.app_name}` を前置する（例: `"${var.app_name}-app-server"`）。`app_name` は prod が `nari-note`、dev が `nari-note-dev`
- SSM パラメータは `/${var.app_name}/<group>/<name>`（例: `/${var.app_name}/db/password`）
- 環境名・バケット名・リージョンをハードコードしない

## main.tf の書き方

`locals` → `resource` の順に書く。`resource` はメインのリソースを先頭に置き、関連リソースをその下に続ける。

```hcl
locals {
  vpc_id = data.terraform_remote_state.vpc.outputs.vpc_id
}

# app/server なら EC2 インスタンスがメイン
resource "aws_instance" "app_server" {
  # ...
}

# EC2 に付随するセキュリティグループ等は下に書く
resource "aws_security_group" "app_server" {
  name   = "${var.app_name}-app-server-sg"
  vpc_id = local.vpc_id
}
```

## remote-resources.tf の書き方

他ユニットの state は `global-variables.tf` の変数で参照する（環境名を埋め込まない）。

```hcl
data "terraform_remote_state" "vpc" {
  backend = "s3"
  config = {
    bucket = var.state_bucket_name
    key    = "${var.env_name}/network/vpc/terraform.tfstate"
    region = var.region
  }
}
```

`data` の値は直接使わず、`main.tf` の `locals` に代入してから使う（上の `local.vpc_id` の例）。
