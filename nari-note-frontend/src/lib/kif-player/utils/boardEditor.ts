import { PIECE_ORDER, ROW_LABELS } from '../constants';
import type { BoardState, CapturedPiece, Piece, PieceType } from '../types';

export const DEMOTE_MAP: Partial<Record<PieceType, PieceType>> = {
  '龍': '飛', '馬': '角', '成銀': '銀', '成桂': '桂', '成香': '香', 'と': '歩',
};

// 持ち駒の数を漢数字に変換するテーブル（最大18枚）
const KANJI_COUNT: readonly string[] = ['', '', '二', '三', '四', '五', '六', '七', '八', '九', '十', '十一', '十二', '十三', '十四', '十五', '十六', '十七', '十八'];

export function deepCopyBoard(board: BoardState): BoardState {
  return board.map(row => row.map(cell => cell ? { ...cell } : null));
}

export function addToCaptured(pieces: CapturedPiece[], type: PieceType): CapturedPiece[] {
  const hit = pieces.find(p => p.type === type);
  if (hit) return pieces.map(p => p.type === type ? { ...p, count: p.count + 1 } : p);
  return [...pieces, { type, count: 1 }];
}

export function removeFromCaptured(pieces: CapturedPiece[], type: PieceType): CapturedPiece[] {
  const hit = pieces.find(p => p.type === type);
  if (!hit) return pieces;
  if (hit.count <= 1) return pieces.filter(p => p.type !== type);
  return pieces.map(p => p.type === type ? { ...p, count: p.count - 1 } : p);
}

export function formatCapturedBOD(pieces: CapturedPiece[]): string {
  const ordered = PIECE_ORDER.flatMap(type => {
    const found = pieces.find(p => p.type === type);
    return found
      ? [found.count === 1 ? found.type : `${found.type}${KANJI_COUNT[found.count]}`]
      : [];
  });
  return ordered.length > 0 ? ordered.join('　') : 'なし';
}

export function generateBOD(
  board: BoardState,
  senteCaptured: CapturedPiece[],
  goteCaptured: CapturedPiece[],
): string {
  const formatRow = (row: (Piece | null)[], label: string) => {
    const cells = row
      .map(p => !p ? ' ・' : p.owner === 'gote' ? `v${p.type}` : ` ${p.type}`)
      .join('');
    return `|${cells}|${label}`;
  };

  return [
    `後手の持駒：${formatCapturedBOD(goteCaptured)}`,
    '  ９ ８ ７ ６ ５ ４ ３ ２ １',
    '+---------------------------+',
    ...board.map((row, i) => formatRow(row, ROW_LABELS[i])),
    '+---------------------------+',
    `先手の持駒：${formatCapturedBOD(senteCaptured)}`,
  ].join('\n');
}
