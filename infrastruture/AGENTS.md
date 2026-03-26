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
