# インフラ設計

## 概要図

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="nari-note-infra-dark.png">
  <img src="nari-note-infra.png" alt="nari-note 本番インフラ構成図: User から Cloudflare Workers 上の Next.js、Cloudflare プロキシを経由して EC2 上の nginx・ASP.NET Core・PostgreSQL に到達する。画像は S3 から CloudFront で配信し、EC2 は SSM・CloudWatch・デプロイ用 S3・EBS と連携する" width="100%">
</picture>

## 各サービス設定

### VPC

### EC2

### RDS

### Redis

### S3

### CloudWatch
