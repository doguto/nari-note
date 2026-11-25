from __future__ import annotations

import argparse
import json
import logging
import os
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from candidates import generate
from judge import Params, judge
from position import board_of, normalize_sfen
from usi import Engine

HERE = Path(__file__).parent
ANALYZE_NODES = 2_000_000
ANALYZE_MULTIPV = 3
ENGINE_OPTIONS = {"USI_Hash": "64"}

log = logging.getLogger("nari-note-engine")


def engine_path() -> str:
    path = os.environ.get("ENGINE_PATH")
    if path:
        return path
    found = sorted(p for p in (HERE / "engine").glob("*") if p.suffix in ("", ".exe") and p.is_file())
    if not found:
        raise RuntimeError("ENGINE_PATH か engine/ にエンジンの実行ファイルを置いてください")
    return str(found[0])


class EngineSlot:
    # 解析と判定でプロセスを分け、判定中でも解析を待たせない
    def __init__(self, cache: Path) -> None:
        self.lock = threading.Lock()
        self.cache = cache
        self.engine: Engine | None = None

    def run(self, fn):
        with self.lock:
            if self.engine is None:
                self.engine = Engine(engine_path(), ENGINE_OPTIONS, self.cache)
            try:
                return fn(self.engine)
            except Exception:
                self.engine.close()
                self.engine = None
                raise


class Handler(BaseHTTPRequestHandler):
    analyzer: EngineSlot
    judger: EngineSlot

    def log_message(self, format, *args):
        log.info("%s - %s", self.address_string(), format % args)

    def _send(self, status: int, data: dict) -> None:
        body = json.dumps(data, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path == "/health":
            return self._send(200, {"status": "ok"})
        self._send(404, {"error": "not found"})

    def do_POST(self):
        try:
            length = int(self.headers.get("Content-Length") or 0)
            body = json.loads(self.rfile.read(length) or b"{}")
            if self.path == "/analyze":
                return self._send(200, self.analyze(body["sfen"]))
            if self.path == "/candidates":
                return self._send(200, self.candidates(body["sfen"], body.get("others", [])))
            if self.path == "/judge":
                return self._send(200, self.judge(body["p"], body["q"]))
            self._send(404, {"error": "not found"})
        except (KeyError, ValueError) as e:
            self._send(400, {"error": str(e)})
        except Exception as e:
            log.exception("request failed")
            self._send(500, {"error": str(e)})

    def analyze(self, sfen: str) -> dict:
        sfen = normalize_sfen(sfen)
        if board_of(sfen).is_game_over():
            return {"lines": []}
        result = self.analyzer.run(lambda e: e.search(sfen, ANALYZE_NODES, multipv=ANALYZE_MULTIPV))
        return {"lines": [{"usi": l.move, "score": l.score, "pv": l.pv} for l in result.lines if l.move != "resign"]}

    def candidates(self, sfen: str, others: list[str]) -> dict:
        target = normalize_sfen(sfen)
        originals = {normalize_sfen(o): o for o in others}
        found = [c for c in generate([target, *originals]) if target in (c.p, c.q)]
        return {
            "candidates": [
                {"sfen": originals[c.q if c.p == target else c.p], "unit_count": len(c.units)} for c in found
            ]
        }

    def judge(self, p: str, q: str) -> dict:
        verdict = self.judger.run(lambda e: judge(e, p, q, Params()))
        return {
            "kind": verdict.kind,
            "delta": verdict.delta,
            "confirmed_depth": verdict.confirmed_depth,
            "reason": verdict.reason,
        }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default=os.environ.get("HOST", "127.0.0.1"))
    parser.add_argument("--port", type=int, default=int(os.environ.get("PORT", "8790")))
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    cache = Path(os.environ.get("ENGINE_CACHE", HERE / "cache.sqlite3"))
    Handler.analyzer = EngineSlot(cache)
    Handler.judger = EngineSlot(cache)
    server = ThreadingHTTPServer((args.host, args.port), Handler)
    log.info("nari-note-engine listening on %s:%d", args.host, args.port)
    server.serve_forever()


if __name__ == "__main__":
    main()
