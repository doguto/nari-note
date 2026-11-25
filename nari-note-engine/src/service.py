from __future__ import annotations

import os
from pathlib import Path
from typing import Callable

from candidates import generate
from judge import Params, judge
from position import board_of, normalize_sfen
from usi import Engine

ENGINE_DIR = Path(__file__).resolve().parent.parent / "engine"
ANALYZE_NODES = 2_000_000
ANALYZE_MULTIPV = 3
ENGINE_OPTIONS = {"USI_Hash": "64"}


def engine_path() -> str:
    path = os.environ.get("ENGINE_PATH")
    if path:
        return path
    found = sorted(p for p in ENGINE_DIR.glob("*") if p.suffix in ("", ".exe") and p.is_file())
    if not found:
        raise RuntimeError("ENGINE_PATH か engine/ にエンジンの実行ファイルを置いてください")
    return str(found[0])


class EngineSlot:
    def __init__(self) -> None:
        self.engine: Engine | None = None

    def run(self, fn: Callable[[Engine], object]):
        if self.engine is None:
            self.engine = Engine(engine_path(), ENGINE_OPTIONS)
        self.engine.memo.clear()
        try:
            return fn(self.engine)
        except Exception:
            # 応答が途中で途切れたエンジンは状態が分からないので、次のリクエストで起動し直す
            self.engine.close()
            self.engine = None
            raise


def analyze(slot: EngineSlot, sfen: str) -> dict:
    sfen = normalize_sfen(sfen)
    if board_of(sfen).is_game_over():
        return {"lines": []}
    result = slot.run(lambda e: e.search(sfen, ANALYZE_NODES, multipv=ANALYZE_MULTIPV))
    return {"lines": [{"usi": l.move, "score": l.score, "pv": l.pv} for l in result.lines if l.move != "resign"]}


def candidates(sfen: str, others: list[str]) -> dict:
    target = normalize_sfen(sfen)
    originals = {normalize_sfen(o): o for o in others}
    found = [c for c in generate([target, *originals]) if target in (c.p, c.q)]
    return {
        "candidates": [
            {"sfen": originals[c.q if c.p == target else c.p], "unit_count": len(c.units)} for c in found
        ]
    }


def judge_pair(slot: EngineSlot, p: str, q: str) -> dict:
    verdict = slot.run(lambda e: judge(e, p, q, Params()))
    return {
        "kind": verdict.kind,
        "delta": verdict.delta,
        "confirmed_depth": verdict.confirmed_depth,
        "reason": verdict.reason,
    }
