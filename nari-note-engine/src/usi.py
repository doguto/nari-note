from __future__ import annotations

import subprocess
import time
from dataclasses import dataclass
from pathlib import Path

MATE_SCORE = 32000


@dataclass
class Line:
    move: str
    score: int
    pv: list[str]


@dataclass
class SearchResult:
    lines: list[Line]
    nodes: int = 0
    nps: int = 0
    elapsed: float = 0.0

    @property
    def best(self) -> Line:
        return self.lines[0]


def _parse_score(tokens: list[str], i: int) -> int:
    kind, value = tokens[i + 1], tokens[i + 2]
    if kind == "cp":
        return int(value)
    if value in ("+", "-"):
        return MATE_SCORE if value == "+" else -MATE_SCORE
    n = int(value)
    return (MATE_SCORE - abs(n)) * (1 if n > 0 or value.startswith("+") else -1)


class Engine:
    def __init__(self, path: str, options: dict[str, str]):
        self.proc = subprocess.Popen(
            [path],
            cwd=str(Path(path).parent),
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            bufsize=1,
        )
        self._send("usi")
        for _ in self._read_until("usiok"):
            pass
        # ノード数を固定した探索を決定的にするため、スレッドは 1 に固定する
        merged = {
            "Threads": "1",
            "USI_Ponder": "false",
            "USI_OwnBook": "false",
            "BookFile": "no_book",
            "PvInterval": "0",
            "ConsiderationMode": "true",
            **options,
        }
        for k, v in merged.items():
            self._send(f"setoption name {k} value {v}")
        self.multipv = None
        self._ready()
        # 1 回の判定の中で同じ探索を繰り返すので、リクエストの間だけ結果を覚えておく
        self.memo: dict[tuple[str, int, int, str], SearchResult] = {}

    def _send(self, cmd: str) -> None:
        self.proc.stdin.write(cmd + "\n")
        self.proc.stdin.flush()

    def _read_until(self, prefix: str):
        while True:
            line = self.proc.stdout.readline()
            if not line:
                raise RuntimeError("エンジンが終了しました")
            line = line.strip()
            yield line
            if line.startswith(prefix):
                return

    def _ready(self) -> None:
        self._send("isready")
        for _ in self._read_until("readyok"):
            pass

    def search(self, sfen: str, nodes: int, multipv: int = 1, searchmoves: list[str] | None = None) -> SearchResult:
        sm = " ".join(searchmoves or [])
        key = (sfen, nodes, multipv, sm)
        if key in self.memo:
            return self.memo[key]

        if self.multipv != multipv:
            self._send(f"setoption name MultiPV value {multipv}")
            self.multipv = multipv
        self._send("usinewgame")
        self._ready()
        self._send(f"position sfen {sfen}")
        cmd = f"go nodes {nodes}" + (f" searchmoves {sm}" if sm else "")
        started = time.perf_counter()
        self._send(cmd)

        blocks: list[list[tuple[int, Line]]] = []
        total_nodes = nps = 0
        bestmove = ""
        for raw in self._read_until("bestmove"):
            tokens = raw.split()
            if tokens[0] == "bestmove":
                bestmove = tokens[1]
                break
            if tokens[0] != "info" or "pv" not in tokens or "score" not in tokens:
                continue
            if "lowerbound" in tokens or "upperbound" in tokens:
                continue
            idx = int(tokens[tokens.index("multipv") + 1]) if "multipv" in tokens else 1
            score = _parse_score(tokens, tokens.index("score"))
            pv = tokens[tokens.index("pv") + 1:]
            depth = int(tokens[tokens.index("depth") + 1]) if "depth" in tokens else 0
            if idx == 1 or not blocks:
                blocks.append([])
            blocks[-1].append((depth, Line(pv[0], score, pv)))
            if "nodes" in tokens:
                total_nodes = int(tokens[tokens.index("nodes") + 1])
            if "nps" in tokens:
                nps = int(tokens[tokens.index("nps") + 1])
        elapsed = time.perf_counter() - started

        # ノード上限で打ち切ると MultiPV の一部の行が前の反復の深さのまま出力されるので、全行の深さがそろった最後の組を使う
        width = max((len(b) for b in blocks), default=0)
        complete = [b for b in blocks if len(b) == width and len({d for d, _ in b}) == 1]
        chosen = complete[-1] if complete else (blocks[-1] if blocks else [])
        ordered = [line for _, line in chosen]
        if not ordered and bestmove not in ("resign", "win"):
            ordered = [Line(bestmove, 0, [bestmove])]
        if bestmove == "resign":
            ordered = [Line("resign", -MATE_SCORE, [])]
        result = SearchResult(ordered, total_nodes, nps, elapsed)
        self.memo[key] = result
        return result

    def close(self) -> None:
        try:
            self._send("quit")
            self.proc.wait(timeout=5)
        except Exception:
            self.proc.kill()
