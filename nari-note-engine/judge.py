from __future__ import annotations

from dataclasses import dataclass, field

from position import (
    DiffUnit,
    board_of,
    diff_units,
    intermediate_positions,
    is_legal_move,
    normalize_sfen,
    push,
)
from usi import Engine, SearchResult


@dataclass
class Params:
    depth: int = 6
    delta: int = 50
    contested_gap: int = 30
    tau_min: int = 50
    nodes: int = 2_000_000
    nodes_intermediate: int = 300_000
    max_units: int = 3


@dataclass
class Ply:
    ply: int
    move: str | None
    score_p: int
    score_q: int
    agreed: bool


@dataclass
class Verdict:
    kind: str  # same / same_with_delta / same_until / different / skipped
    units: list[str]
    effective: list[list[str]] = field(default_factory=list)
    confirmed_depth: int = 0
    delta: int | None = None
    plies: list[Ply] = field(default_factory=list)
    reason: str = ""


def _contest(r: SearchResult) -> int:
    if len(r.lines) < 2:
        return 0
    return r.lines[0].score - r.lines[1].score


def _regret(engine: Engine, sfen: str, r: SearchResult, move: str, params: Params) -> int | None:
    if not is_legal_move(board_of(sfen), move):
        return None
    for line in r.lines:
        if line.move == move:
            return r.best.score - line.score
    forced = engine.search(sfen, params.nodes, searchmoves=[move])
    return r.best.score - forced.best.score


def best_agrees(
    engine: Engine, p_sfen: str, q_sfen: str, rp: SearchResult, rq: SearchResult, params: Params
) -> str | None:
    if rp.best.move == rq.best.move:
        return rp.best.move
    if _contest(rp) >= params.contested_gap and _contest(rq) >= params.contested_gap:
        return None
    # MultiPV=2 だけでは 30cp 以内の手の集合を取りこぼすので、相手の最善手を searchmoves で直接評価する
    regret_q = _regret(engine, q_sfen, rq, rp.best.move, params)
    regret_p = _regret(engine, p_sfen, rp, rq.best.move, params)
    options = []
    if regret_q is not None and regret_q < params.contested_gap:
        options.append((regret_q, rp.best.move))
    if regret_p is not None and regret_p < params.contested_gap:
        options.append((regret_p, rq.best.move))
    return min(options)[1] if options else None


def check_units(
    engine: Engine,
    p_sfen: str,
    units: list[DiffUnit],
    move: str,
    tau: int,
    params: Params,
) -> list[list[str]]:
    failing = []
    for subset, sfen in intermediate_positions(board_of(p_sfen), units):
        labels = [u.label() for u in subset]
        if not is_legal_move(board_of(sfen), move):
            failing.append(labels)
            continue
        free = engine.search(sfen, params.nodes_intermediate)
        forced = engine.search(sfen, params.nodes_intermediate, searchmoves=[move])
        if free.best.score - forced.best.score >= tau:
            failing.append(labels)
    # 他の失敗集合を包含する集合は原因として冗長なので、極小なものだけ残す
    minimal = [f for f in failing if not any(set(g) < set(f) for g in failing)]
    return minimal


def judge(engine: Engine, p_sfen: str, q_sfen: str, params: Params) -> Verdict:
    p_sfen, q_sfen = normalize_sfen(p_sfen), normalize_sfen(q_sfen)
    p, q = board_of(p_sfen), board_of(q_sfen)
    if p.turn != q.turn:
        return Verdict("skipped", [], reason="手番が違います")
    units = diff_units(p, q)
    labels = [u.label() for u in units]
    if not units:
        return Verdict("same", [], confirmed_depth=params.depth, delta=0, reason="同一局面です")
    if len(units) > params.max_units:
        return Verdict("skipped", labels, reason=f"差分が {len(units)} 単位あり、上限 {params.max_units} を超えています")

    plies: list[Ply] = []
    deltas: list[int] = []
    cur_p, cur_q = p_sfen, q_sfen
    for ply in range(params.depth):
        rp = engine.search(cur_p, params.nodes, multipv=2)
        rq = engine.search(cur_q, params.nodes, multipv=2)
        move = best_agrees(engine, cur_p, cur_q, rp, rq, params)
        plies.append(Ply(ply, move, rp.best.score, rq.best.score, move is not None))

        if move is None or not is_legal_move(board_of(cur_q), move):
            cur_units = diff_units(board_of(cur_p), board_of(cur_q))
            tau = max(params.tau_min, 2 * max(_contest(rp), _contest(rq), 0))
            effective = check_units(engine, cur_p, cur_units, rp.best.move, tau, params) if len(cur_units) <= params.max_units else []
            kind = "different" if ply == 0 else "same_until"
            return Verdict(kind, labels, effective, ply, None, plies, reason=f"{ply} 手目で最善手が分かれました")

        if ply == 0:
            tau = max(params.tau_min, 2 * max(_contest(rp), _contest(rq), 0))
            effective = check_units(engine, cur_p, units, move, tau, params)
            if effective:
                return Verdict("different", labels, effective, 0, None, plies, reason="差分の一部で最善手が入れ替わります")

        # 評価値は手番側から見た値なので、元の局面の手番側に揃えてから差を取る
        sign = 1 if ply % 2 == 0 else -1
        deltas.append(sign * (rp.best.score - rq.best.score))
        cur_p, cur_q = push(cur_p, move), push(cur_q, move)

    if all(abs(d) <= params.delta for d in deltas):
        return Verdict("same", labels, [], params.depth, round(sum(deltas) / len(deltas)), plies)
    if max(deltas) - min(deltas) <= params.delta:
        return Verdict("same_with_delta", labels, [], params.depth, round(sum(deltas) / len(deltas)), plies)
    effective = [labels] if len(labels) == 1 else []
    return Verdict("different", labels, effective, params.depth, None, plies, reason="手順は同じですが評価値の差が一定しません")
