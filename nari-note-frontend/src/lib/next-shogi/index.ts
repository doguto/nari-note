/**
 * next-shogi - BOD形式将棋盤面レンダリングライブラリ
 *
 * BOD形式の将棋盤面をパースして、React コンポーネントとしてレンダリングします。
 */

// 型定義
export type {
  PieceType,
  PieceOwner,
  Piece as PieceData,
  BoardState,
  CapturedPiece as CapturedPieceData,
  CapturedPieces as CapturedPiecesData,
  ParsedBoard,
  BoardSize,
} from './types';

// コンポーネント
export {
  ShogiBoard,
  Board,
  Piece as PieceComponent,
  CapturedPieces as CapturedPiecesComponent,
} from './components';

// ユーティリティ
export { parseBOD, parseCapturedPieces } from './utils';

// 定数
export {
  PIECE_MAP,
  KANJI_NUMBER_MAP,
  EMPTY_SQUARE,
  PIECE_ORDER,
  ROW_LABELS,
  COLUMN_LABELS,
} from './constants';
