export { KifPlayer, ShogiBoard, Board, Piece, CapturedPieces } from './components';
export { parseKif, getBoardAtMove, parseBOD, parseCapturedPieces, deepCopyBoard, addToCaptured, removeFromCaptured, formatCapturedBOD, generateBOD, DEMOTE_MAP } from './utils';
export { useBoardEditor } from './hooks/useBoardEditor';
export type { BoardEditorSelection, BoardEditorState } from './hooks/useBoardEditor';
export { useFreePlayRecorder } from './hooks/useFreePlayRecorder';
export type { FreePlayMove, FreePlayRecorderState, PendingPromotion } from './hooks/useFreePlayRecorder';
export type {
  KifMove,
  KifGame,
  PieceType,
  PieceOwner,
  Piece as PieceData,
  BoardState,
  CapturedPiece,
  CapturedPieces as CapturedPiecesData,
  ParsedBoard,
  BoardSize,
} from './types';
