output "api_log_group_name" {
  value = aws_cloudwatch_log_group.api.name
}

output "system_log_group_name" {
  value = aws_cloudwatch_log_group.system.name
}

output "dashboard_name" {
  value = aws_cloudwatch_dashboard.main.dashboard_name
}
