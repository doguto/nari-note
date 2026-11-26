import type { BoardState, CapturedPiece, PieceOwner, PieceType } from '@/lib/kif-player';

const BASE: Record<string, PieceType> = {
  P: '歩', L: '香', N: '桂', S: '銀', G: '金', B: '角', R: '飛', K: '玉',
};
const PROMOTED: Record<string, PieceType> = {
  P: 'と', L: '成香', N: '成桂', S: '成銀', B: '馬', R: '龍',
};
const HAND_ORDER = ['R', 'B', 'G', 'S', 'N', 'L', 'P'];
const RANKS = 'abcdefghi';

export interface ParsedSfen {
  board: BoardState;
  captured: { sente: CapturedPiece[]; gote: CapturedPiece[] };
  turn: PieceOwner;
}

export function parseSfen(sfen: string): ParsedSfen {
  const [boardPart, turnPart, handPart] = sfen.split(' ');
  const board: BoardState = boardPart.split('/').map((row) => {
    const cells: BoardState[number] = [];
    let promoted = false;
    for (const ch of row) {
      if (ch >= '1' && ch <= '9') {
        for (let i = 0; i < Number(ch); i++) cells.push(null);
        continue;
      }
      if (ch === '+') {
        promoted = true;
        continue;
      }
      const letter = ch.toUpperCase();
      const owner: PieceOwner = ch === letter ? 'sente' : 'gote';
      let type = promoted ? PROMOTED[letter] : BASE[letter];
      if (type === '玉' && owner === 'gote') type = '王';
      cells.push({ type, owner });
      promoted = false;
    }
    return cells;
  });

  const counts: Record<PieceOwner, Record<string, number>> = { sente: {}, gote: {} };
  if (handPart && handPart !== '-') {
    let num = '';
    for (const ch of handPart) {
      if (ch >= '0' && ch <= '9') {
        num += ch;
        continue;
      }
      const letter = ch.toUpperCase();
      const owner: PieceOwner = ch === letter ? 'sente' : 'gote';
      counts[owner][letter] = (counts[owner][letter] ?? 0) + (num ? Number(num) : 1);
      num = '';
    }
  }
  const toCaptured = (owner: PieceOwner): CapturedPiece[] =>
    HAND_ORDER.filter((l) => counts[owner][l]).map((l) => ({ type: BASE[l], count: counts[owner][l] }));

  return {
    board,
    captured: { sente: toCaptured('sente'), gote: toCaptured('gote') },
    turn: turnPart === 'w' ? 'gote' : 'sente',
  };
}

// Board の行列は row 0 = 一段目、col 0 = 9 筋
export function usiSquare(row: number, col: number): string {
  return `${9 - col}${RANKS[row]}`;
}

export function cellOfUsiSquare(square: string): { row: number; col: number } {
  return { row: RANKS.indexOf(square[1]), col: 9 - Number(square[0]) };
}

const SFEN_LETTER: Record<PieceType, string> = {
  '玉': 'K', '王': 'K', '飛': 'R', '角': 'B', '金': 'G', '銀': 'S', '桂': 'N', '香': 'L', '歩': 'P',
  '龍': '+R', '馬': '+B', '成銀': '+S', '成桂': '+N', '成香': '+L', 'と': '+P',
};

// 盤面エディタで作った局面を、手番を付けて SFEN にする
export function boardToSfen(
  board: BoardState,
  captured: { sente: CapturedPiece[]; gote: CapturedPiece[] },
  turn: PieceOwner,
): string {
  const rows = board.map((row) => {
    let text = '';
    let empty = 0;
    for (const piece of row) {
      if (!piece) {
        empty++;
        continue;
      }
      if (empty) text += empty;
      empty = 0;
      const letter = SFEN_LETTER[piece.type];
      text += piece.owner === 'sente' ? letter : letter.toLowerCase();
    }
    return text + (empty ? empty : '');
  });

  const hand = (['sente', 'gote'] as const)
    .flatMap((owner) =>
      HAND_ORDER.map((l) => {
        const count = captured[owner].find((p) => p.type === BASE[l])?.count ?? 0;
        if (!count) return '';
        const letter = owner === 'sente' ? l : l.toLowerCase();
        return `${count > 1 ? count : ''}${letter}`;
      }),
    )
    .join('');

  return `${rows.join('/')} ${turn === 'sente' ? 'b' : 'w'} ${hand || '-'} 1`;
}

export function dropLetter(type: PieceType): string | undefined {
  return Object.entries(BASE).find(([, t]) => t === type)?.[0];
}
