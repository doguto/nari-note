# .env の TF_VAR_ 変数を環境変数にセットして terragrunt run -a apply を実行する
param(
    [string]$EnvFile = ".env"
)

$envFilePath = Join-Path $PSScriptRoot $EnvFile

if (-not (Test-Path $envFilePath)) {
    Write-Error ".env ファイルが見つからないわ: $envFilePath"
    exit 1
}

Get-Content $envFilePath | ForEach-Object {
    $line = $_.Trim()

    # 空行・コメント行はスキップ
    if ($line -eq "" -or $line.StartsWith("#")) { return }

    $parts = $line -split "=", 2
    if ($parts.Length -ne 2) { return }

    $key   = $parts[0].Trim()
    $value = $parts[1].Trim().Trim('"').Trim("'")

    [System.Environment]::SetEnvironmentVariable($key, $value, "Process")
    Write-Host "  $key = $value"
}

Write-Host ""
aws-vault exec narinote -- terragrunt run -a apply
