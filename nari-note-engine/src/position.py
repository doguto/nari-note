from __future__ import annotations

import itertools
from dataclasses import dataclass

import cshogi

BASE_LETTERS = {1: "P", 2: "L", 3: "N", 4: "S", 5: "B", 6: "R", 7: "G", 8: "K"}
HAND_BASES = [1, 2, 3, 4, 7, 5, 6]  # cshogi の持ち駒の並び（P L N S G B R）
BLACK, WHITE = 0, 1
HAND = -1

STEP_GOLD = [(0, -1), (1, -1), (-1, -1), (1, 0), (-1, 0), (0, 1)]
STEP_KING = [(df, dr) for df in (-1, 0, 1) for dr in (-1, 0, 1) if (df, dr) != (0, 0)]
STEPS = {
    1: [(0, -1)],
    3: [(1, -2), (-1, -2)],
    4: [(0, -1), (1, -1), (-1, -1), (1, 1), (-1, 1)],
    7: STEP_GOLD,
    8: STEP_KING,
}
DIAG = [(1, 1), (1, -1), (-1, 1), (-1, -1)]
ORTHO = [(0, 1), (0, -1), (1, 0), (-1, 0)]


@dataclass(frozen=True, order=True)
class Token:
    base: int
    color: int
    sq: int
    promoted: bool

    def label(self) -> str:
        side = "b" if self.color == BLACK else "w"
        letter = ("+" if self.promoted else "") + BASE_LETTERS[self.base]
        return f"{side}{letter}@{'hand' if self.sq == HAND else square_name(self.sq)}"


@dataclass(frozen=True)
class DiffUnit:
    src: Token
    dst: Token

    def label(self) -> str:
        return f"{self.src.label()}->{self.dst.label()}"


def square_name(sq: int) -> str:
    return f"{sq // 9 + 1}{'abcdefghi'[sq % 9]}"


def parse_square(name: str) -> int:
    return (int(name[0]) - 1) * 9 + "abcdefghi".index(name[1])


def normalize_sfen(sfen: str) -> str:
    parts = sfen.split()
    return " ".join(parts[:3] + ["1"])


def board_of(sfen: str) -> cshogi.Board:
    return cshogi.Board(normalize_sfen(sfen))


def decode(piece: int) -> tuple[int, int, bool]:
    color = WHITE if piece >= 16 else BLACK
    p = piece - 16 if color == WHITE else piece
    if p > 8:
        return p - 8, color, True
    return p, color, False


def encode(base: int, color: int, promoted: bool) -> int:
    return base + (8 if promoted else 0) + (16 if color == WHITE else 0)


def tokens_of(board: cshogi.Board) -> list[Token]:
    tokens = []
    for sq, piece in enumerate(board.pieces):
        if piece:
            base, color, promoted = decode(piece)
            tokens.append(Token(base, color, sq, promoted))
    for color, hand in enumerate(board.pieces_in_hand):
        for i, n in enumerate(hand):
            tokens.extend(Token(HAND_BASES[i], color, HAND, False) for _ in range(n))
    return tokens


def material_key(board: cshogi.Board) -> tuple[int, ...]:
    counts = [0] * 9
    for t in tokens_of(board):
        if t.color == BLACK:
            counts[t.base] += 1
    return tuple(counts[1:8])


def material_distance(a: tuple[int, ...], b: tuple[int, ...]) -> int:
    return sum(abs(x - y) for x, y in zip(a, b))


def _unit_cost(src: Token, dst: Token) -> int:
    cost = 0 if src.color == dst.color else 100
    if src.sq == HAND or dst.sq == HAND:
        return cost + (0 if src.sq == dst.sq else 20)
    return cost + abs(src.sq // 9 - dst.sq // 9) + abs(src.sq % 9 - dst.sq % 9)


def diff_units(p: cshogi.Board, q: cshogi.Board) -> list[DiffUnit]:
    remaining = list(tokens_of(q))
    only_p = []
    for t in tokens_of(p):
        if t in remaining:
            remaining.remove(t)
        else:
            only_p.append(t)
    only_q = remaining

    units = []
    for base in range(1, 9):
        src = [t for t in only_p if t.base == base]
        dst = [t for t in only_q if t.base == base]
        if len(src) != len(dst):
            raise ValueError("駒の総数が一致しない局面は比較できません")
        if not src:
            continue
        # 同種の駒は数枚しかないので全順列で最小コストの対応を取る
        best = min(
            itertools.permutations(dst),
            key=lambda perm: sum(_unit_cost(s, d) for s, d in zip(src, perm)),
        )
        units.extend(DiffUnit(s, d) for s, d in zip(src, best))
    return units


def to_sfen(tokens: list[Token], turn: int) -> str:
    grid = [0] * 81
    hands = [[0] * 7, [0] * 7]
    for t in tokens:
        if t.sq == HAND:
            hands[t.color][HAND_BASES.index(t.base)] += 1
        else:
            grid[t.sq] = encode(t.base, t.color, t.promoted)

    rows = []
    for rank in range(9):
        row, empty = "", 0
        for file in range(8, -1, -1):
            piece = grid[file * 9 + rank]
            if not piece:
                empty += 1
                continue
            if empty:
                row, empty = row + str(empty), 0
            base, color, promoted = decode(piece)
            letter = BASE_LETTERS[base]
            row += ("+" if promoted else "") + (letter.lower() if color == WHITE else letter)
        rows.append(row + (str(empty) if empty else ""))

    hand_order = [6, 5, 7, 4, 3, 2, 1]  # R B G S N L P
    hand = ""
    for color in (BLACK, WHITE):
        for base in hand_order:
            n = hands[color][HAND_BASES.index(base)]
            if n:
                letter = BASE_LETTERS[base] if color == BLACK else BASE_LETTERS[base].lower()
                hand += (str(n) if n > 1 else "") + letter
    return f"{'/'.join(rows)} {'b' if turn == BLACK else 'w'} {hand or '-'} 1"


def apply_units(p: cshogi.Board, units: list[DiffUnit]) -> str:
    tokens = tokens_of(p)
    for u in units:
        tokens.remove(u.src)
        tokens.append(u.dst)
    return to_sfen(tokens, p.turn)


def is_legal_position(sfen: str) -> bool:
    board = board_of(sfen)
    pieces = board.pieces
    for color in (BLACK, WHITE):
        for file in range(9):
            pawns = 0
            for rank in range(9):
                piece = pieces[file * 9 + rank]
                if not piece:
                    continue
                base, c, promoted = decode(piece)
                if c != color or promoted:
                    continue
                depth = rank if color == BLACK else 8 - rank
                if base in (1, 2) and depth == 0:
                    return False
                if base == 3 and depth <= 1:
                    return False
                if base == 1:
                    pawns += 1
            if pawns > 1:
                return False
    parts = sfen.split()
    parts[1] = "w" if parts[1] == "b" else "b"
    return not cshogi.Board(" ".join(parts)).is_check()


def intermediate_positions(p: cshogi.Board, units: list[DiffUnit]) -> list[tuple[list[DiffUnit], str]]:
    result = []
    for size in range(1, len(units)):
        for subset in itertools.combinations(units, size):
            sfen = apply_units(p, list(subset))
            if is_legal_position(sfen):
                result.append((list(subset), sfen))
    return result


def _directions(base: int, promoted: bool) -> tuple[list[tuple[int, int]], list[tuple[int, int]]]:
    if promoted and base in (1, 2, 3, 4):
        return STEP_GOLD, []
    if base == 2:
        return [], [(0, -1)]
    if base == 5:
        return (ORTHO if promoted else []), DIAG
    if base == 6:
        return (DIAG if promoted else []), ORTHO
    return STEPS[base], []


def attackers(board: cshogi.Board, target: int, color: int) -> int:
    pieces = board.pieces
    tf, tr = target // 9, target % 9
    count = 0
    for sq, piece in enumerate(pieces):
        if not piece:
            continue
        base, c, promoted = decode(piece)
        if c != color:
            continue
        sign = 1 if color == BLACK else -1
        steps, slides = _directions(base, promoted)
        f, r = sq // 9, sq % 9
        hit = any((f + df, r + dr * sign) == (tf, tr) for df, dr in steps)
        for df, dr in slides:
            if hit:
                break
            nf, nr = f + df, r + dr * sign
            while 0 <= nf < 9 and 0 <= nr < 9:
                if (nf, nr) == (tf, tr):
                    hit = True
                    break
                if pieces[nf * 9 + nr]:
                    break
                nf, nr = nf + df, nr + dr * sign
        count += hit
    return count


def move_squares(usi: str) -> list[int]:
    if usi[1] == "*":
        return [parse_square(usi[2:4])]
    return [parse_square(usi[0:2]), parse_square(usi[2:4])]


def attack_signature(board: cshogi.Board, usi: str) -> tuple[int, ...]:
    return tuple(attackers(board, sq, c) for sq in move_squares(usi) for c in (BLACK, WHITE))


def is_legal_move(board: cshogi.Board, usi: str) -> bool:
    try:
        move = board.move_from_usi(usi)
    except Exception:
        return False
    return bool(move) and board.is_legal(move)


def push(sfen: str, usi: str) -> str:
    board = board_of(sfen)
    board.push_usi(usi)
    return normalize_sfen(board.sfen())
