# セットアップ手順

## 1. AWS認証情報の取得
### 1-1. AWS アカウントの作成

AWSの公式サイトからアカウントを作成する

### 1-2. IAMユーザーの作成

rootユーザーでAWSにログインし、IAMサービスから新しいユーザーを作成する
AdministratorAccessの権限を付与する

### 1-3. Access Keyの作成

コンソールからアクセスキーを作成し、`AWS_ACCESS_KEY_ID` と `AWS_SECRET_ACCESS_KEY` を取得する

## 2. aws-vaultのセットアップ
### 2-1. wingetを使用してインストール

```bash
winget install -e --id 99designs.aws-vault
```

### 2-2. aws-vaultに認証情報を追加

```bash
aws-vault add profile-name
```

## 3. terraformのセットアップ
### 3-1. tenvのインストール

```bash
winget install -e --id Tofuutils.Tenv
```

### 3-2. tenvを使用してインストール

```bash
tenv tf install
tenv tg install
```
