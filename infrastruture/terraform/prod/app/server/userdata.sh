#!/bin/bash
set -e

# パッケージの更新
dnf update -y

# nginx
dnf install -y nginx

# nginx の自動起動を有効化&起動
systemctl enable --now nginx


# Application
dnf install -y aspnetcore-runtime-9.0
