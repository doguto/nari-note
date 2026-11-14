import os
import time
from datetime import date, datetime, timedelta, timezone

import boto3

logs = boto3.client("logs")
s3 = boto3.client("s3")

BUCKET_NAME = os.environ["BUCKET_NAME"]
LOG_GROUP_NAMES = os.environ["LOG_GROUP_NAMES"].split(",")
POLL_INTERVAL_SECONDS = 10
# 次のポーリングまでに Lambda のタイムアウトを迎えないための余裕
SAFETY_MARGIN_MILLIS = 30_000

# CloudWatch Logs がバケットへの書き込み権限を確かめるために置くオブジェクト
WRITE_TEST_OBJECT = "aws-logs-write-test"


def handler(event, context):
    day = target_day(event)
    start = datetime(day.year, day.month, day.day, tzinfo=timezone.utc)
    end = start + timedelta(days=1)

    for log_group_name in LOG_GROUP_NAMES:
        prefix = f"{log_group_name.strip('/')}/{day:%Y/%m/%d}"
        task_id = create_export_task(log_group_name, start, end, prefix, context)
        if task_id is None:
            print(f"Skipped {log_group_name} {day}: already exported")
            continue
        wait_for_completion(task_id, context)
        print(f"Exported {log_group_name} {day} to s3://{BUCKET_NAME}/{prefix}")


# 手動で再実行するときは {"date": "YYYY-MM-DD"} で対象日 (UTC) を指定する。CloudWatch Logs の保持期間内に限る
def target_day(event):
    if isinstance(event, dict) and event.get("date"):
        return date.fromisoformat(event["date"])
    return datetime.now(timezone.utc).date() - timedelta(days=1)


# 非同期呼び出しの自動リトライで同じ日を重複して出力しないよう、出力済みのロググループは飛ばす
def already_exported(prefix):
    response = s3.list_objects_v2(Bucket=BUCKET_NAME, Prefix=f"{prefix}/")
    return any(not obj["Key"].endswith(WRITE_TEST_OBJECT) for obj in response.get("Contents", []))


# エクスポートタスクはアカウント・リージョンごとに同時に 1 つしか実行できないため、実行中のものがあれば終わるまで待つ
def create_export_task(log_group_name, start, end, prefix, context):
    while True:
        if already_exported(prefix):
            return None
        try:
            response = logs.create_export_task(
                logGroupName=log_group_name,
                fromTime=to_millis(start),
                to=to_millis(end),
                destination=BUCKET_NAME,
                destinationPrefix=prefix,
            )
            return response["taskId"]
        except logs.exceptions.LimitExceededException:
            sleep_or_timeout(context, f"Another export task is running: {log_group_name}")


def wait_for_completion(task_id, context):
    while True:
        task = logs.describe_export_tasks(taskId=task_id)["exportTasks"][0]
        status = task["status"]
        if status["code"] == "COMPLETED":
            return
        if status["code"] in ("CANCELLED", "FAILED"):
            raise RuntimeError(f"Export task {task_id} {status['code']}: {status.get('message', '')}")
        sleep_or_timeout(context, f"Export task {task_id} did not complete")


def sleep_or_timeout(context, message):
    if context.get_remaining_time_in_millis() < POLL_INTERVAL_SECONDS * 1000 + SAFETY_MARGIN_MILLIS:
        raise TimeoutError(message)
    time.sleep(POLL_INTERVAL_SECONDS)


def to_millis(value):
    return int(value.timestamp() * 1000)
