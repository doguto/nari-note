#!/bin/bash
set -e

# システムの更新
dnf update -y

# .NET 9 SDKのインストール
dnf install -y dotnet-sdk-9.0

# CloudWatch エージェントのインストール
dnf install -y amazon-cloudwatch-agent

# CloudWatch エージェントの設定ファイル作成
cat > /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json << 'CWCONFIG'
{
  "logs": {
    "logs_collected": {
      "files": {
        "collect_list": [
          {
            "file_path": "/var/log/nari-note/app.log",
            "log_group_name": "/nari-note/api",
            "log_stream_name": "{instance_id}/app",
            "timestamp_format": "%Y-%m-%dT%H:%M:%S"
          }
        ]
      }
    }
  },
  "metrics": {
    "metrics_collected": {
      "mem": {
        "measurement": ["mem_used_percent"]
      },
      "disk": {
        "measurement": ["used_percent"],
        "resources": ["/"]
      }
    }
  }
}
CWCONFIG

# ログディレクトリ作成
mkdir -p /var/log/nari-note

# CloudWatch エージェントの起動
systemctl enable amazon-cloudwatch-agent
systemctl start amazon-cloudwatch-agent
