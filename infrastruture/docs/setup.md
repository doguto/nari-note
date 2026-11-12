# セットアップ手順

インフラ操作・バックエンドのデプロイを手元から行うための準備。手順は Windows (winget) を前提とする。

## 1. AWS 認証情報

1. AWS コンソールで IAM ユーザーを作成し、必要な権限 (現状は AdministratorAccess) を付与する
2. そのユーザーのアクセスキーを発行する
3. aws-vault に登録する。プロファイル名は `narinote` (スクリプトの既定値。変える場合は `scripts/.env` の `AWS_VAULT_PROFILE`)

```bash
winget install -e --id 99designs.aws-vault
winget install -e --id Amazon.AWSCLI
aws-vault add narinote
```

## 2. Terraform / Terragrunt

tenv でインストールする。バージョンは `infrastruture/terraform/.terraform-version` / `.terragrunt-version` に従う。

```bash
winget install -e --id Tofuutils.Tenv
cd infrastruture/terraform
tenv tf install
tenv tg install
```

## 3. 実行方法

各ユニットのディレクトリで、aws-vault 経由で実行する。

```bash
cd infrastruture/terraform/prod/app/server
aws-vault exec narinote -- terragrunt plan
aws-vault exec narinote -- terragrunt apply
```

- 秘匿値が必要なユニットはディレクトリに `terraform.tfvars` を置く (Git 管理外。値は管理者から受け取る)
- state 用バケット (`terraform/backend/`) と予算アラート (`terraform/budget/`) は Terragrunt を使わない素の Terraform で、`terraform` コマンドで操作する

## 4. デプロイ・運用に使うもの

- バックエンドのデプロイ (`scripts/deploy.sh`) には bash (Windows では Git Bash) と .NET SDK が必要
- EC2 への接続は SSM Session Manager で行う (SSH は閉じている)。AWS CLI に [Session Manager プラグイン](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html) を入れておく
- 対象環境・インスタンスを変える場合は `scripts/.env.example` を `scripts/.env` にコピーして編集する

デプロイの流れは [infra-design.md](infra-design.md#デプロイとリリース) を参照。
