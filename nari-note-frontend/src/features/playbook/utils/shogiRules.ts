// 研究盤で手元だけで指し進めるための将棋のルール。バックエンドの ShogiBoard / ShogiNotation と同じ結果を返す
// マスの番号は (筋 - 1) * 9 + 段（一段目が 0）

export type Side = 'b' | 'w';
export type Kind = 'P' | 'L' | 'N' | 'S' | 'G' | 'B' | 'R' | 'K';

interface Piece {
  kind: Kind;
  side: Side;
  promoted: boolean;
}

export interface Position {
  board: (Piece | null)[];
  hands: Record<Side, Record<Kind, number>>;
  turn: Side;
}

interface Move {
  from: number | null;
  to: number;
  drop: Kind | null;
  promote: boolean;
}

const HAND_ORDER: Kind[] = ['R', 'B', 'G', 'S', 'N', 'L', 'P'];
const KINDS: Kind[] = ['P', 'L', 'N', 'S', 'G', 'B', 'R', 'K'];
const GOLD: [number, number][] = [[0, -1], [1, -1], [-1, -1], [1, 0], [-1, 0], [0, 1]];
const SILVER: [number, number][] = [[0, -1], [1, -1], [-1, -1], [1, 1], [-1, 1]];
const KING: [number, number][] = [[0, -1], [1, -1], [-1, -1], [1, 0], [-1, 0], [0, 1], [1, 1], [-1, 1]];
const KNIGHT: [number, number][] = [[1, -2], [-1, -2]];
const DIAGONALS: [number, number][] = [[1, 1], [1, -1], [-1, 1], [-1, -1]];
const ORTHOGONALS: [number, number][] = [[0, 1], [0, -1], [1, 0], [-1, 0]];

const FULL_WIDTH = '０１２３４５６７８９';
const KANJI = '〇一二三四五六七八九';
const NAMES: Record<Kind, string> = { P: '歩', L: '香', N: '桂', S: '銀', G: '金', B: '角', R: '飛', K: '玉' };
const PROMOTED_NAMES: Partial<Record<Kind, string>> = { P: 'と', L: '成香', N: '成桂', S: '成銀', B: '馬', R: '龍' };

const opponent = (side: Side): Side => (side === 'b' ? 'w' : 'b');
const emptyHand = (): Record<Kind, number> => ({ P: 0, L: 0, N: 0, S: 0, G: 0, B: 0, R: 0, K: 0 });

export function parseSquare(name: string): number {
  return (Number(name[0]) - 1) * 9 + (name.charCodeAt(1) - 97);
}

function squareName(sq: number): string {
  return `${Math.floor(sq / 9) + 1}${String.fromCharCode(97 + (sq % 9))}`;
}

export function fromSfen(sfen: string): Position {
  const [boardPart, turnPart, handPart] = sfen.trim().split(/\s+/);
  const board: (Piece | null)[] = new Array(81).fill(null);
  boardPart.split('/').forEach((row, rank) => {
    let file = 9;
    let promoted = false;
    for (const ch of row) {
      if (ch >= '1' && ch <= '9') {
        file -= Number(ch);
        continue;
      }
      if (ch === '+') {
        promoted = true;
        continue;
      }
      const kind = ch.toUpperCase() as Kind;
      board[(file - 1) * 9 + rank] = { kind, side: ch === kind ? 'b' : 'w', promoted };
      promoted = false;
      file--;
    }
  });
  const hands = { b: emptyHand(), w: emptyHand() };
  if (handPart && handPart !== '-') {
    let count = 0;
    for (const ch of handPart) {
      if (ch >= '0' && ch <= '9') {
        count = count * 10 + Number(ch);
        continue;
      }
      const kind = ch.toUpperCase() as Kind;
      hands[ch === kind ? 'b' : 'w'][kind] += Math.max(count, 1);
      count = 0;
    }
  }
  return { board, hands, turn: turnPart === 'w' ? 'w' : 'b' };
}

export function toSfen(position: Position): string {
  const rows: string[] = [];
  for (let rank = 0; rank < 9; rank++) {
    let row = '';
    let empty = 0;
    for (let file = 9; file >= 1; file--) {
      const piece = position.board[(file - 1) * 9 + rank];
      if (!piece) {
        empty++;
        continue;
      }
      if (empty > 0) row += empty;
      empty = 0;
      row += (piece.promoted ? '+' : '') + (piece.side === 'b' ? piece.kind : piece.kind.toLowerCase());
    }
    rows.push(row + (empty > 0 ? empty : ''));
  }
  let hand = '';
  for (const side of ['b', 'w'] as const) {
    for (const kind of HAND_ORDER) {
      const n = position.hands[side][kind];
      if (n === 0) continue;
      hand += (n > 1 ? n : '') + (side === 'b' ? kind : kind.toLowerCase());
    }
  }
  return `${rows.join('/')} ${position.turn} ${hand || '-'} 1`;
}

function parseMove(usi: string): Move {
  if (usi[1] === '*') return { from: null, to: parseSquare(usi.slice(2, 4)), drop: usi[0].toUpperCase() as Kind, promote: false };
  return { from: parseSquare(usi.slice(0, 2)), to: parseSquare(usi.slice(2, 4)), drop: null, promote: usi.length === 5 };
}

function toUsi(move: Move): string {
  if (move.drop) return `${move.drop}*${squareName(move.to)}`;
  return squareName(move.from!) + squareName(move.to) + (move.promote ? '+' : '');
}

function apply(position: Position, move: Move): Position {
  const board = [...position.board];
  const hands = { b: { ...position.hands.b }, w: { ...position.hands.w } };
  if (move.drop) {
    hands[position.turn][move.drop]--;
    board[move.to] = { kind: move.drop, side: position.turn, promoted: false };
  } else {
    const piece = board[move.from!]!;
    const captured = board[move.to];
    if (captured) hands[position.turn][captured.kind]++;
    board[move.from!] = null;
    board[move.to] = { ...piece, promoted: piece.promoted || move.promote };
  }
  return { board, hands, turn: opponent(position.turn) };
}

function destinations(position: Position, from: number, piece: Piece): number[] {
  const result: number[] = [];
  const sign = piece.side === 'b' ? 1 : -1;
  const file = Math.floor(from / 9);
  const rank = from % 9;
  const inside = (f: number, r: number) => f >= 0 && f < 9 && r >= 0 && r < 9;
  const step = (steps: [number, number][]) => {
    for (const [df, dr] of steps) {
      const f = file + df;
      const r = rank + dr * sign;
      if (inside(f, r)) result.push(f * 9 + r);
    }
  };
  const slide = (dirs: [number, number][]) => {
    for (const [df, dr] of dirs) {
      let f = file + df;
      let r = rank + dr * sign;
      while (inside(f, r)) {
        result.push(f * 9 + r);
        if (position.board[f * 9 + r]) break;
        f += df;
        r += dr * sign;
      }
    }
  };

  if (piece.promoted && (piece.kind === 'P' || piece.kind === 'L' || piece.kind === 'N' || piece.kind === 'S')) {
    step(GOLD);
    return result;
  }
  switch (piece.kind) {
    case 'P': step([[0, -1]]); break;
    case 'L': slide([[0, -1]]); break;
    case 'N': step(KNIGHT); break;
    case 'S': step(SILVER); break;
    case 'G': step(GOLD); break;
    case 'K': step(KING); break;
    case 'B': slide(DIAGONALS); if (piece.promoted) step(ORTHOGONALS); break;
    case 'R': slide(ORTHOGONALS); if (piece.promoted) step(DIAGONALS); break;
  }
  return result;
}

const inZone = (sq: number, side: Side) => (side === 'b' ? sq % 9 <= 2 : sq % 9 >= 6);

function mustPromote(piece: Piece, to: number): boolean {
  if (piece.promoted) return false;
  const depth = piece.side === 'b' ? to % 9 : 8 - (to % 9);
  if (piece.kind === 'P' || piece.kind === 'L') return depth === 0;
  if (piece.kind === 'N') return depth <= 1;
  return false;
}

const canPromote = (piece: Piece) => !piece.promoted && piece.kind !== 'G' && piece.kind !== 'K';

function pseudoLegalMoves(position: Position): Move[] {
  const moves: Move[] = [];
  const turn = position.turn;
  position.board.forEach((piece, sq) => {
    if (!piece || piece.side !== turn) return;
    for (const to of destinations(position, sq, piece)) {
      const target = position.board[to];
      if (target && (target.side === turn || target.kind === 'K')) continue;
      if (canPromote(piece) && (inZone(sq, turn) || inZone(to, turn))) moves.push({ from: sq, to, drop: null, promote: true });
      if (!mustPromote(piece, to)) moves.push({ from: sq, to, drop: null, promote: false });
    }
  });
  for (const kind of KINDS) {
    if (kind === 'K' || position.hands[turn][kind] === 0) continue;
    for (let to = 0; to < 81; to++) {
      if (position.board[to]) continue;
      if (mustPromote({ kind, side: turn, promoted: false }, to)) continue;
      if (kind === 'P' && hasPawnOnFile(position, Math.floor(to / 9), turn)) continue;
      moves.push({ from: null, to, drop: kind, promote: false });
    }
  }
  return moves;
}

function hasPawnOnFile(position: Position, fileIndex: number, side: Side): boolean {
  for (let rank = 0; rank < 9; rank++) {
    const p = position.board[fileIndex * 9 + rank];
    if (p && p.kind === 'P' && !p.promoted && p.side === side) return true;
  }
  return false;
}

function isAttacked(position: Position, target: number, by: Side): boolean {
  return position.board.some((piece, sq) => piece && piece.side === by && destinations(position, sq, piece).includes(target));
}

function isInCheck(position: Position, side: Side): boolean {
  const king = position.board.findIndex((p) => p && p.kind === 'K' && p.side === side);
  return king >= 0 && isAttacked(position, king, opponent(side));
}

function hasEscape(position: Position): boolean {
  return pseudoLegalMoves(position).some((m) => !isInCheck(apply(position, m), position.turn));
}

function legal(position: Position): Move[] {
  return pseudoLegalMoves(position).filter((m) => {
    const next = apply(position, m);
    if (isInCheck(next, position.turn)) return false;
    // 打ち歩詰め
    if (m.drop === 'P' && isInCheck(next, next.turn) && !hasEscape(next)) return false;
    return true;
  });
}

export function legalMoves(sfen: string): string[] {
  return legal(fromSfen(sfen)).map(toUsi);
}

// 指した後の局面。合法かどうかは呼び出し側で legalMoves と照らし合わせる
export function play(sfen: string, usi: string): string {
  return toSfen(apply(fromSfen(sfen), parseMove(usi)));
}

const squareLabel = (sq: number) => `${FULL_WIDTH[Math.floor(sq / 9) + 1]}${KANJI[(sq % 9) + 1]}`;
const pieceName = (kind: Kind, promoted: boolean) => (promoted ? PROMOTED_NAMES[kind] ?? NAMES[kind] : NAMES[kind]);

// 曖昧な指し手は KIF と同じく移動元を括弧で添える
export function moveLabel(sfen: string, usi: string, previousTo?: number): string {
  const position = fromSfen(sfen);
  const move = parseMove(usi);
  const mark = position.turn === 'b' ? '▲' : '△';
  const destination = previousTo === move.to ? '同　' : squareLabel(move.to);
  const moves = legal(position);
  const same = (a: Move, b: Move) => a.from === b.from && a.to === b.to && a.drop === b.drop && a.promote === b.promote;

  if (move.drop) {
    const boardAlternative = moves.some((m) => {
      if (m.drop || m.to !== move.to) return false;
      const p = position.board[m.from!];
      return !!p && p.kind === move.drop && !p.promoted;
    });
    return `${mark}${destination}${NAMES[move.drop]}${boardAlternative ? '打' : ''}`;
  }

  const piece = position.board[move.from!]!;
  let label = `${mark}${destination}${pieceName(piece.kind, piece.promoted)}`;
  const ambiguous = moves.some((m) => {
    if (m.drop || m.to !== move.to || m.from === move.from) return false;
    const p = position.board[m.from!];
    return !!p && p.kind === piece.kind && p.side === piece.side && p.promoted === piece.promoted;
  });
  if (ambiguous) label += `(${Math.floor(move.from! / 9) + 1}${(move.from! % 9) + 1})`;
  if (move.promote) return `${label}成`;
  if (moves.some((m) => same(m, { ...move, promote: true }))) return `${label}不成`;
  return label;
}

export function destinationOf(usi: string): number {
  return parseSquare(usi.slice(2, 4));
}
