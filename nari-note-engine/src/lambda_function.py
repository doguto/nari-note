from __future__ import annotations

import service

# 実行環境が再利用される間はエンジンのプロセスを使い回し、評価関数の読み込みを省く
slots = {engine: service.EngineSlot(engine) for engine in service.ENGINES}


def handler(event: dict, context) -> dict:
    action = event.get("action")
    if action == "analyze":
        engine = event["engine"]
        if engine not in slots:
            raise ValueError(f"未対応のエンジンです: {engine}")
        return service.analyze(slots[engine], event["sfen"])
    if action == "candidates":
        return service.candidates(event["sfen"], event.get("others", []))
    if action == "judge":
        return service.judge_pair(slots[service.JUDGE_ENGINE], event["p"], event["q"])
    raise ValueError(f"未対応の action です: {action}")
