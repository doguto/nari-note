from __future__ import annotations

import os
from pathlib import Path
from typing import Callable

from position import board_of, normalize_sfen
from usi import Engine

SRC_DIR = Path(__file__).resolve().parent
# Lambda の zip では engine/ がこのファイルと同じ階層に、リポジトリでは src/ の隣にある
ENGINE_DIR = SRC_DIR / "engine" if (SRC_DIR / "engine").is_dir() else SRC_DIR.parent / "engine"
ANALYZE_NODES = 2_000_000
ANALYZE_MULTIPV = 3
ENGINES = ("tsuyouraou-mini", "tsuyouraou-micro")
ENGINE_OPTIONS = {"USI_Hash": "64"}
# つようら王の作者が示す FV_SCALE の最適値
EVAL_OPTIONS = {"FV_SCALE": "24"}


def engine_command(engine: str) -> tuple[str, dict[str, str]]:
    if engine not in ENGINES:
        raise ValueError(f"未対応のエンジンです: {engine}")
    directory = ENGINE_DIR / engine
    found = [p for p in (directory / "YaneuraOu-by-gcc", *sorted(directory.glob("*.exe"))) if p.is_file()]
    if found:
        return str(found[0]), {**ENGINE_OPTIONS, **EVAL_OPTIONS}
    # 評価関数を埋め込んだエンジンなどで手元の動作だけ確かめるときは、どのエンジンを選んでもこれを使う
    path = os.environ.get("ENGINE_PATH")
    if path:
        return path, ENGINE_OPTIONS
    raise RuntimeError(f"engine/{engine}/ にエンジンの実行ファイルを置くか、ENGINE_PATH を指定してください")


class EngineSlot:
    def __init__(self, engine: str) -> None:
        self.name = engine
        self.engine: Engine | None = None

    def run(self, fn: Callable[[Engine], object]):
        if self.engine is None:
            self.engine = Engine(*engine_command(self.name))
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

