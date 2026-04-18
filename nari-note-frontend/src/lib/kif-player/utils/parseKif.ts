import type { PieceType } from '@/lib/next-shogi/types';
import type { KifGame, KifMove } from '../types';

const FULL_WIDTH_TO_NUM: Record<string, number> = {
  '１': 1, '２': 2, '３': 3, '４': 4, '５': 5,
  '６': 6, '７': 7, '８': 8, '９': 9,
};

const KANJI_TO_NUM: Record<string, number> = {
  '一': 1, '二': 2, '三': 3, '四': 4, '五': 5,
  '六': 6, '七': 7, '八': 8, '九': 9,
};

const TERMINAL_KEYWORDS = ['投了', '千日手', '中断', '持将棋', '時間切れ', '入玉'];

// Groups:
//   1: 手数
//   2: 同（前手と同じ座標）
//   3: 列（全角数字）
//   4: 段（漢数字）
//   5: 駒種（成銀/成桂/成香 または 1文字）
//   6: 成フラグ
//   7: 打フラグ
//   8: fromCol
//   9: fromRow
const MOVE_RE =
  /^\s*(\d+)\s+(?:(同[　\s]?)|([１-９])([一二三四五六七八九]))(成[銀桂香]|[玉王飛角金銀桂香歩龍馬と])(成)?(打)?(?:\((\d)(\d)\))?/u;

export function parseKif(kifText: string): KifGame {
  const lines = kifText.split('\n');
  const game: KifGame = { moves: [] };
  let lastToCol = 1;
  let lastToRow = 1;

  for (const line of lines) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('*')) continue;

    if (trimmed.startsWith('先手：')) {
      game.sentePlayer = trimmed.slice(3).trim();
      continue;
    }
    if (trimmed.startsWith('後手：')) {
      game.gotePlayer = trimmed.slice(3).trim();
      continue;
    }

    const numMatch = /^\s*(\d+)\s+/.exec(line);
    if (!numMatch) continue;

    const afterNum = line.slice(numMatch[0].length).trimStart();

    if (TERMINAL_KEYWORDS.some((kw) => afterNum.startsWith(kw))) break;

    const m = MOVE_RE.exec(line);
    if (!m) continue;

    const moveNumber = parseInt(m[1], 10);
    const isSameSquare = !!m[2];
    const toCol = isSameSquare ? lastToCol : FULL_WIDTH_TO_NUM[m[3]] ?? 1;
    const toRow = isSameSquare ? lastToRow : KANJI_TO_NUM[m[4]] ?? 1;
    const piece = m[5] as PieceType;
    const isPromote = !!m[6];
    const isDrop = !!m[7];
    const fromCol = m[8] ? parseInt(m[8], 10) : undefined;
    const fromRow = m[9] ? parseInt(m[9], 10) : undefined;

    const move: KifMove = {
      moveNumber,
      toCol,
      toRow,
      piece,
      fromCol,
      fromRow,
      isDrop,
      isPromote,
      isSameSquare,
    };

    game.moves.push(move);
    lastToCol = toCol;
    lastToRow = toRow;
  }

  return game;
}
