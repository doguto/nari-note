from __future__ import annotations

from collections import defaultdict
from dataclasses import dataclass

from position import (
    attack_signature,
    board_of,
    diff_units,
    is_legal_move,
    material_distance,
    material_key,
    normalize_sfen,
)


@dataclass
class Candidate:
    p: str
    q: str
    units: list[str]
    material_diff: int


def _move_compatible(p_sfen: str, q_sfen: str, moves: dict[str, str]) -> bool:
    for src, dst in ((p_sfen, q_sfen), (q_sfen, p_sfen)):
        move = moves.get(src)
        if not move:
            continue
        src_board, dst_board = board_of(src), board_of(dst)
        if not is_legal_move(dst_board, move):
            return False
        if attack_signature(src_board, move) != attack_signature(dst_board, move):
            return False
    return True


def generate(
    sfens: list[str],
    moves: dict[str, str] | None = None,
    max_units: int = 3,
    max_material: int = 1,
) -> list[Candidate]:
    moves = {normalize_sfen(k): v for k, v in (moves or {}).items()}
    uniq = sorted({normalize_sfen(s) for s in sfens})
    boards = {s: board_of(s) for s in uniq}

    # 駒割の差は 1 枚以内なので、手番ごとに駒割キーでまとめてから近いバケット同士だけ比べる
    buckets: dict[int, dict[tuple[int, ...], list[str]]] = defaultdict(lambda: defaultdict(list))
    for s, b in boards.items():
        buckets[b.turn][material_key(b)].append(s)

    result = []
    for by_material in buckets.values():
        keys = sorted(by_material)
        for i, ka in enumerate(keys):
            for kb in keys[i:]:
                md = material_distance(ka, kb)
                if md > max_material:
                    continue
                left, right = by_material[ka], by_material[kb]
                pairs = (
                    ((left[x], left[y]) for x in range(len(left)) for y in range(x + 1, len(left)))
                    if ka == kb
                    else ((a, b) for a in left for b in right)
                )
                for a, b in pairs:
                    units = diff_units(boards[a], boards[b])
                    if not units or len(units) > max_units:
                        continue
                    if not _move_compatible(a, b, moves):
                        continue
                    result.append(Candidate(a, b, [u.label() for u in units], md))
    return result
