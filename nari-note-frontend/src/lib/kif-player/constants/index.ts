import type { BoardState, Piece } from '@/lib/next-shogi/types';

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
