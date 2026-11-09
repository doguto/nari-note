import type { BoardState, CapturedPieces, ParsedBoard, PieceOwner, PieceType } from '../types';
import type { KifGame, KifMove } from '../types';
import { INITIAL_BOARD } from '../constants';

const PROMOTE_MAP: Partial<Record<PieceType, PieceType>> = {
  '飛': '龍', '角': '馬', '銀': '成銀', '桂': '成桂', '香': '成香', '歩': 'と',
};

const DEMOTE_MAP: Partial<Record<PieceType, PieceType>> = {
  '龍': '飛', '馬': '角', '成銀': '銀', '成桂': '桂', '成香': '香', 'と': '歩',
};

function colIdx(kifCol: number): number { return 9 - kifCol; }
function rowIdx(kifRow: number): number { return kifRow - 1; }

function deepCopyBoard(board: BoardState): BoardState {
  return board.map((row) => row.map((cell) => (cell ? { ...cell } : null)));
}

function addCaptured(captured: CapturedPieces, pieceType: PieceType, by: 'sente' | 'gote'): void {
  const list = by === 'sente' ? captured.sente : captured.gote;
  const demoted = DEMOTE_MAP[pieceType] ?? pieceType;
  const existing = list.find((p) => p.type === demoted);
  if (existing) {
    existing.count++;
  } else {
    list.push({ type: demoted, count: 1 });
  }
}

function removeCaptured(captured: CapturedPieces, pieceType: PieceType, by: 'sente' | 'gote'): void {
  const list = by === 'sente' ? captured.sente : captured.gote;
  const existing = list.find((p) => p.type === pieceType);
  if (!existing) return;
  existing.count--;
  if (existing.count <= 0) list.splice(list.indexOf(existing), 1);
}

// 1手目を指す側から数えて、N手目を指す側を返す
export function moverOf(moveNumber: number, firstTurn: PieceOwner): PieceOwner {
  const isFirstSide = moveNumber % 2 === 1;
  if (firstTurn === 'sente') return isFirstSide ? 'sente' : 'gote';
  return isFirstSide ? 'gote' : 'sente';
}

function applyMove(board: BoardState, captured: CapturedPieces, move: KifMove, firstTurn: PieceOwner): void {
  const owner = moverOf(move.moveNumber, firstTurn);
  const toCi = colIdx(move.toCol);
  const toRi = rowIdx(move.toRow);

  if (move.isDrop) {
    removeCaptured(captured, move.piece, owner);
    board[toRi][toCi] = { type: move.piece, owner };
    return;
  }

  const fromCi = colIdx(move.fromCol!);
  const fromRi = rowIdx(move.fromRow!);

  const target = board[toRi][toCi];
  if (target && target.owner !== owner) {
    addCaptured(captured, target.type, owner);
  }

  const finalType: PieceType = move.isPromote ? (PROMOTE_MAP[move.piece] ?? move.piece) : move.piece;
  board[fromRi][fromCi] = null;
  board[toRi][toCi] = { type: finalType, owner };
}

// moveNumber=0: 初期配置, N: N手目指了後の盤面
export function getBoardAtMove(game: KifGame, moveNumber: number): ParsedBoard {
  const initial = game.initialPosition;
  const board = deepCopyBoard(initial?.board ?? INITIAL_BOARD);
  const captured: CapturedPieces = {
    sente: (initial?.captured.sente ?? []).map((p) => ({ ...p })),
    gote: (initial?.captured.gote ?? []).map((p) => ({ ...p })),
  };

  const limit = Math.min(moveNumber, game.moves.length);
  for (let i = 0; i < limit; i++) {
    applyMove(board, captured, game.moves[i], game.firstTurn);
  }

  return { board, captured, sentePlayer: game.sentePlayer, gotePlayer: game.gotePlayer };
}
