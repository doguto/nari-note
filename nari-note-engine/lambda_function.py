from __future__ import annotations

import service

# 実行環境が再利用される間はエンジンのプロセスを使い回し、評価関数の読み込みを省く
slot = service.EngineSlot()


def handler(event: dict, context) -> dict:
    action = event.get("action")
    if action == "analyze":
        return service.analyze(slot, event["sfen"])
    if action == "candidates":
        return service.candidates(event["sfen"], event.get("others", []))
    if action == "judge":
        return service.judge_pair(slot, event["p"], event["q"])
    raise ValueError(f"未対応の action です: {action}")
