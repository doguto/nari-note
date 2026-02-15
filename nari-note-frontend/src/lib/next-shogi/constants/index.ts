import type { Piece, PieceType } from '../types';

/**
 * BOD形式の駒表記から駒情報へのマッピング
 */
export const PIECE_MAP: Record<string, Piece> = {
  // 先手の駒
  '玉': { type: '玉', owner: 'sente' },
  '飛': { type: '飛', owner: 'sente' },
  '角': { type: '角', owner: 'sente' },
  '金': { type: '金', owner: 'sente' },
  '銀': { type: '銀', owner: 'sente' },
  '桂': { type: '桂', owner: 'sente' },
  '香': { type: '香', owner: 'sente' },
  '歩': { type: '歩', owner: 'sente' },
  '龍': { type: '龍', owner: 'sente' },
  '馬': { type: '馬', owner: 'sente' },
  '成銀': { type: '成銀', owner: 'sente' },
  '成桂': { type: '成桂', owner: 'sente' },
  '成香': { type: '成香', owner: 'sente' },
  'と': { type: 'と', owner: 'sente' },

  // 後手の駒（v接頭辞付き）
  'v玉': { type: '玉', owner: 'gote' },
  'v王': { type: '王', owner: 'gote' },
  'v飛': { type: '飛', owner: 'gote' },
  'v角': { type: '角', owner: 'gote' },
  'v金': { type: '金', owner: 'gote' },
  'v銀': { type: '銀', owner: 'gote' },
  'v桂': { type: '桂', owner: 'gote' },
  'v香': { type: '香', owner: 'gote' },
  'v歩': { type: '歩', owner: 'gote' },
  'v龍': { type: '龍', owner: 'gote' },
  'v馬': { type: '馬', owner: 'gote' },
  'v成銀': { type: '成銀', owner: 'gote' },
  'v成桂': { type: '成桂', owner: 'gote' },
  'v成香': { type: '成香', owner: 'gote' },
  'vと': { type: 'と', owner: 'gote' },
};

/**
 * 漢数字から数値へのマッピング
 */
export const KANJI_NUMBER_MAP: Record<string, number> = {
  '〇': 0,
  '零': 0,
  '一': 1,
  '二': 2,
  '三': 3,
  '四': 4,
  '五': 5,
  '六': 6,
  '七': 7,
  '八': 8,
  '九': 9,
  '十': 10,
};

/**
 * 空マスを表す記号
 */
export const EMPTY_SQUARE = '・';

/**
 * 駒の表示順序（持駒表示用）
 */
export const PIECE_ORDER: PieceType[] = [
  '飛',
  '角',
  '金',
  '銀',
  '桂',
  '香',
  '歩',
];

/**
 * 行番号の漢数字
 */
export const ROW_LABELS = ['一', '二', '三', '四', '五', '六', '七', '八', '九'];

/**
 * 列番号
 */
export const COLUMN_LABELS = ['9', '8', '7', '6', '5', '4', '3', '2', '1'];
