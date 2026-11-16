output "log_export_function_name" {
  value = aws_lambda_function.log_export.function_name
}

output "log_export_role_name" {
  value = aws_iam_role.log_export.name
}
