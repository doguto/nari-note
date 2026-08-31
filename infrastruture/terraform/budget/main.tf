resource "aws_budgets_budget" "monthly-budget" {
  name        = "nari-note-monthly-budget"
  budget_type = "COST"

  # $45/month (約7000円弱/月)に設定
  limit_amount = 45
  limit_unit   = "USD"
  time_unit    = "MONTHLY"

  notification {
    comparison_operator = "GREATER_THAN"
    notification_type   = "ACTUAL"

    # 75%を超えたら通知する
    threshold      = 90.0
    threshold_type = "PERCENTAGE"

    subscriber_email_addresses = [var.alert_email]
  }
}
