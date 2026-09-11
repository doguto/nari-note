locals {
  instance_id = data.terraform_remote_state.server.outputs.instance_id
}

resource "aws_sns_topic" "alerts" {
  name = "${var.app_name}-alerts"
}

resource "aws_sns_topic_subscription" "alerts_email" {
  topic_arn = aws_sns_topic.alerts.arn
  protocol  = "email"
  endpoint  = var.alert_email
}

resource "aws_cloudwatch_metric_alarm" "app_server_cpu" {
  alarm_name          = "${var.app_name}-app-server-cpu-utilization"
  alarm_description   = "EC2 instance CPU utilization exceeded threshold"
  namespace           = "AWS/EC2"
  metric_name         = "CPUUtilization"
  statistic           = "Average"
  period              = var.cpu_alarm_period
  evaluation_periods  = var.cpu_alarm_evaluation_periods
  threshold           = var.cpu_alarm_threshold
  comparison_operator = "GreaterThanThreshold"

  dimensions = {
    InstanceId = local.instance_id
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
  ok_actions    = [aws_sns_topic.alerts.arn]

  tags = {
    Name = "${var.app_name}-app-server-cpu-alarm"
  }
}
