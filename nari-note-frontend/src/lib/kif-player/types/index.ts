export type PieceType =
  | '玉' | '王' | '飛' | '角' | '金' | '銀' | '桂' | '香' | '歩'
  | '龍' | '馬' | '成銀' | '成桂' | '成香' | 'と';

export type PieceOwner = 'sente' | 'gote';

export interface Piece {
  type: PieceType;
  owner: PieceOwner;
}

export type BoardState = (Piece | null)[][];

export interface CapturedPiece {
  type: PieceType;
  count: number;
}

export interface CapturedPieces {
  sente: CapturedPiece[];
  gote: CapturedPiece[];
}

export interface ParsedBoard {
  board: BoardState;
  captured: CapturedPieces;
  sentePlayer?: string;
  gotePlayer?: string;
}

export type BoardSize = 'sm' | 'md' | 'lg';

export interface KifMove {
  moveNumber: number;
  toCol: number;
  toRow: number;
  piece: PieceType;
  fromCol?: number;
  fromRow?: number;
  isDrop: boolean;
  isPromote: boolean;
  isSameSquare: boolean;
}

export interface KifGame {
  sentePlayer?: string;
  gotePlayer?: string;
  moves: KifMove[];
}
