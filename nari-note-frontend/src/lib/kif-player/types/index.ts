import type { PieceType } from '@/lib/next-shogi/types';

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
