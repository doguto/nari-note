/**
 * 将棋の駒の種類
 */
export type PieceType =
  | '玉' // 玉将（先手）
  | '王' // 王将（後手）
  | '飛' // 飛車
  | '角' // 角行
  | '金' // 金将
  | '銀' // 銀将
  | '桂' // 桂馬
  | '香' // 香車
  | '歩' // 歩兵
  | '龍' // 竜王（飛車の成り駒）
  | '馬' // 竜馬（角行の成り駒）
  | '成銀' // 成銀
  | '成桂' // 成桂
  | '成香' // 成香
  | 'と'; // と金（歩の成り駒）

/**
 * 駒の所有者
 */
export type PieceOwner = 'sente' | 'gote';

/**
 * 盤上の駒を表す型
 */
export interface Piece {
  type: PieceType;
  owner: PieceOwner;
}

/**
 * 9x9の将棋盤面を表す型
 * null は空マスを示す
 */
export type BoardState = (Piece | null)[][];

/**
 * 持駒の情報
 */
export interface CapturedPiece {
  type: PieceType;
  count: number;
}

/**
 * 先手と後手の持駒
 */
export interface CapturedPieces {
  sente: CapturedPiece[];
  gote: CapturedPiece[];
}

/**
 * BOD形式をパースした結果
 */
export interface ParsedBoard {
  board: BoardState;
  captured: CapturedPieces;
  sentePlayer?: string;
  gotePlayer?: string;
}

/**
 * コンポーネントのサイズオプション
 */
export type BoardSize = 'sm' | 'md' | 'lg';
