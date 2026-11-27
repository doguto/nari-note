from __future__ import annotations

import cshogi


def normalize_sfen(sfen: str) -> str:
    parts = sfen.split()
    return " ".join(parts[:3] + ["1"])


def board_of(sfen: str) -> cshogi.Board:
    return cshogi.Board(normalize_sfen(sfen))
