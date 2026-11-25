from __future__ import annotations

import argparse
import json
import logging
import os
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

import service

log = logging.getLogger("nari-note-engine")


class LockedSlot(service.EngineSlot):
    # 解析と判定でプロセスを分け、判定中でも解析を待たせない。同じプロセスへの要求は直列にする
    def __init__(self) -> None:
        super().__init__()
        self.lock = threading.Lock()

    def run(self, fn):
        with self.lock:
            return super().run(fn)


class Handler(BaseHTTPRequestHandler):
    analyzer: LockedSlot
    judger: LockedSlot

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
                return self._send(200, service.analyze(self.analyzer, body["sfen"]))
            if self.path == "/candidates":
                return self._send(200, service.candidates(body["sfen"], body.get("others", [])))
            if self.path == "/judge":
                return self._send(200, service.judge_pair(self.judger, body["p"], body["q"]))
            self._send(404, {"error": "not found"})
        except (KeyError, ValueError) as e:
            self._send(400, {"error": str(e)})
        except Exception as e:
            log.exception("request failed")
            self._send(500, {"error": str(e)})


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default=os.environ.get("HOST", "127.0.0.1"))
    parser.add_argument("--port", type=int, default=int(os.environ.get("PORT", "8790")))
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    Handler.analyzer = LockedSlot()
    Handler.judger = LockedSlot()
    server = ThreadingHTTPServer((args.host, args.port), Handler)
    log.info("nari-note-engine listening on %s:%d", args.host, args.port)
    server.serve_forever()


if __name__ == "__main__":
    main()
