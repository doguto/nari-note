import type { BoardState, Piece, PieceType } from '../types';

const s = (type: Piece['type']): Piece => ({ type, owner: 'sente' });
const g = (type: Piece['type']): Piece => ({ type, owner: 'gote' });
const _ = null;

// 平手初期配置
// board[rowIdx][colIdx]: rowIdx 0=一段目, colIdx 0=9筋〜colIdx 8=1筋
export const INITIAL_BOARD: BoardState = [
  [g('香'), g('桂'), g('銀'), g('金'), g('王'), g('金'), g('銀'), g('桂'), g('香')],
  [_,       g('飛'), _,       _,       _,       _,       _,       g('角'), _      ],
  [g('歩'), g('歩'), g('歩'), g('歩'), g('歩'), g('歩'), g('歩'), g('歩'), g('歩')],
  [_,       _,       _,       _,       _,       _,       _,       _,       _      ],
  [_,       _,       _,       _,       _,       _,       _,       _,       _      ],
  [_,       _,       _,       _,       _,       _,       _,       _,       _      ],
  [s('歩'), s('歩'), s('歩'), s('歩'), s('歩'), s('歩'), s('歩'), s('歩'), s('歩')],
  [_,       s('角'), _,       _,       _,       _,       _,       s('飛'), _      ],
  [s('香'), s('桂'), s('銀'), s('金'), s('玉'), s('金'), s('銀'), s('桂'), s('香')],
];

export const ROW_LABELS = ['一', '二', '三', '四', '五', '六', '七', '八', '九'];
export const COLUMN_LABELS = ['9', '8', '7', '6', '5', '4', '3', '2', '1'];

export const PIECE_ORDER: PieceType[] = ['飛', '角', '金', '銀', '桂', '香', '歩'];

export const PIECE_MAP: Record<string, Piece> = {
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

export const KANJI_NUMBER_MAP: Record<string, number> = {
  '〇': 0, '零': 0,
  '一': 1, '二': 2, '三': 3, '四': 4, '五': 5,
  '六': 6, '七': 7, '八': 8, '九': 9, '十': 10,
};

export const EMPTY_SQUARE = '・';
