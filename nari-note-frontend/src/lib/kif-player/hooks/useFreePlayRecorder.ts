'use client';

import { useState, useCallback } from 'react';
import { deepCopyBoard, addToCaptured, removeFromCaptured, DEMOTE_MAP, generateBOD } from '../utils/boardEditor';
import type { BoardState, CapturedPiece, PieceOwner, PieceType } from '../types';
import type { BoardEditorSelection } from './useBoardEditor';

const PROMOTE_MAP: Partial<Record<PieceType, PieceType>> = {
  '飛': '龍', '角': '馬', '銀': '成銀', '桂': '成桂', '香': '成香', '歩': 'と',
};

const FW_COL = ['', '１', '２', '３', '４', '５', '６', '７', '８', '９'];
const KJ_ROW = ['', '一', '二', '三', '四', '五', '六', '七', '八', '九'];

const toKifRow = (boardRow: number) => boardRow + 1;
const toKifCol = (boardCol: number) => 9 - boardCol;

function inPromotionZone(boardRow: number, owner: PieceOwner): boolean {
  return owner === 'sente' ? boardRow <= 2 : boardRow >= 6;
}

function mustPromote(piece: PieceType, boardRow: number, owner: PieceOwner): boolean {
  if (owner === 'sente') {
    if ((piece === '歩' || piece === '香') && boardRow === 0) return true;
    if (piece === '桂' && boardRow <= 1) return true;
  } else {
    if ((piece === '歩' || piece === '香') && boardRow === 8) return true;
    if (piece === '桂' && boardRow >= 7) return true;
  }
  return false;
}

export interface FreePlayMove {
  kifRow: number;
  kifCol: number;
  fromKifRow?: number;
  fromKifCol?: number;
  prePiece: PieceType;
  isDrop: boolean;
  isPromote: boolean;
}

export interface PendingPromotion {
  boardRow: number;
  boardCol: number;
  fromBoardRow: number;
  fromBoardCol: number;
  prePiece: PieceType;
  owner: PieceOwner;
}

export interface FreePlayRecorderState {
  board: BoardState;
  senteCaptured: CapturedPiece[];
  goteCaptured: CapturedPiece[];
  selected: BoardEditorSelection | null;
  moveHistory: FreePlayMove[];
  pendingPromotion: PendingPromotion | null;
  handleBoardClick: (row: number, col: number) => void;
  handleHandClick: (owner: PieceOwner, type: PieceType) => void;
  handleHandZoneClick: (owner: PieceOwner) => void;
  confirmPromotion: (promote: boolean) => void;
  isSelectedBoard: (row: number, col: number) => boolean;
  isSelectedHand: (owner: PieceOwner, type: PieceType) => boolean;
  initializeBoard: (board: BoardState, sente: CapturedPiece[], gote: CapturedPiece[]) => void;
  generateKIF: (originalKifText: string, originalMoveCount: number) => string;
  generateCurrentBOD: () => string;
}

function formatMove(move: FreePlayMove, moveNum: number, prev?: FreePlayMove): string {
  const isSame = prev !== undefined && move.kifRow === prev.kifRow && move.kifCol === prev.kifCol;
  const to = isSame ? '同　' : `${FW_COL[move.kifCol]}${KJ_ROW[move.kifRow]}`;
  const promote = move.isPromote ? '成' : '';
  const from = move.isDrop ? '打' : `(${move.fromKifCol}${move.fromKifRow})`;
  return `   ${moveNum} ${to}${move.prePiece}${promote}${from}`;
}

export function useFreePlayRecorder(): FreePlayRecorderState {
  const [board, setBoard]                 = useState<BoardState>([]);
  const [senteCaptured, setSenteCaptured] = useState<CapturedPiece[]>([]);
  const [goteCaptured,  setGoteCaptured]  = useState<CapturedPiece[]>([]);
  const [selected, setSelected]           = useState<BoardEditorSelection | null>(null);
  const [moveHistory, setMoveHistory]     = useState<FreePlayMove[]>([]);
  const [pendingPromotion, setPendingPromotion] = useState<PendingPromotion | null>(null);

  const initializeBoard = useCallback((newBoard: BoardState, sente: CapturedPiece[], gote: CapturedPiece[]) => {
    setBoard(deepCopyBoard(newBoard));
    setSenteCaptured(sente.map(p => ({ ...p })));
    setGoteCaptured(gote.map(p => ({ ...p })));
    setSelected(null);
    setMoveHistory([]);
    setPendingPromotion(null);
  }, []);

  const recordAndApplyMove = useCallback((
    newBoard: BoardState,
    newSente: CapturedPiece[],
    newGote: CapturedPiece[],
    move: Omit<FreePlayMove, 'isPromote'>,
    toRow: number,
    toCol: number,
    fromRow: number | undefined,
    fromCol: number | undefined,
    piece: PieceType,
    owner: PieceOwner,
  ) => {
    setBoard(newBoard);
    setSenteCaptured(newSente);
    setGoteCaptured(newGote);
    setSelected(null);

    const canPromote = !!PROMOTE_MAP[piece];
    const shouldMustPromote = mustPromote(piece, toRow, owner);

    if (canPromote && shouldMustPromote) {
      // 強制成り
      const newB = deepCopyBoard(newBoard);
      newB[toRow][toCol] = { type: PROMOTE_MAP[piece]!, owner };
      setBoard(newB);
      setMoveHistory(prev => [...prev, { ...move, isPromote: true }]);
    } else if (
      canPromote &&
      !move.isDrop &&
      (inPromotionZone(toRow, owner) || (fromRow !== undefined && inPromotionZone(fromRow, owner)))
    ) {
      // 任意成り → 確認待ち
      setPendingPromotion({
        boardRow: toRow,
        boardCol: toCol,
        fromBoardRow: fromRow!,
        fromBoardCol: fromCol!,
        prePiece: piece,
        owner,
      });
    } else {
      setMoveHistory(prev => [...prev, { ...move, isPromote: false }]);
    }
  }, []);

  const handleBoardClick = useCallback((row: number, col: number) => {
    if (pendingPromotion) return;
    const piece = board[row]?.[col];

    if (!selected) {
      if (piece) setSelected({ from: 'board', row, col, piece });
      return;
    }

    if (selected.from === 'board' && selected.row === row && selected.col === col) {
      setSelected(null);
      return;
    }

    const newBoard = deepCopyBoard(board);
    let newSente = [...senteCaptured.map(p => ({ ...p }))];
    let newGote  = [...goteCaptured.map(p => ({ ...p }))];

    if (selected.from === 'board') {
      const movingPiece = selected.piece;
      const displaced   = newBoard[row][col];
      newBoard[selected.row][selected.col] = null;
      newBoard[row][col] = movingPiece;

      if (displaced && displaced.owner !== movingPiece.owner) {
        const capturedType = (DEMOTE_MAP[displaced.type] ?? displaced.type) as PieceType;
        if (movingPiece.owner === 'sente') {
          newSente = addToCaptured(newSente, capturedType);
        } else {
          newGote = addToCaptured(newGote, capturedType);
        }
      }

      const move: Omit<FreePlayMove, 'isPromote'> = {
        kifRow: toKifRow(row),
        kifCol: toKifCol(col),
        fromKifRow: toKifRow(selected.row),
        fromKifCol: toKifCol(selected.col),
        prePiece: movingPiece.type,
        isDrop: false,
      };

      recordAndApplyMove(newBoard, newSente, newGote, move, row, col, selected.row, selected.col, movingPiece.type, movingPiece.owner);
    } else {
      // 持ち駒打ち
      const displaced = newBoard[row][col];
      newBoard[row][col] = { type: selected.type, owner: selected.owner };

      if (selected.owner === 'sente') {
        newSente = removeFromCaptured(newSente, selected.type);
      } else {
        newGote = removeFromCaptured(newGote, selected.type);
      }

      if (displaced) {
        const capturedType = (DEMOTE_MAP[displaced.type] ?? displaced.type) as PieceType;
        if (selected.owner === 'sente') {
          newSente = addToCaptured(newSente, capturedType);
        } else {
          newGote = addToCaptured(newGote, capturedType);
        }
      }

      const move: Omit<FreePlayMove, 'isPromote'> = {
        kifRow: toKifRow(row),
        kifCol: toKifCol(col),
        prePiece: selected.type,
        isDrop: true,
      };

      recordAndApplyMove(newBoard, newSente, newGote, move, row, col, undefined, undefined, selected.type, selected.owner);
    }
  }, [board, senteCaptured, goteCaptured, selected, pendingPromotion, recordAndApplyMove]);

  const handleHandZoneClick = useCallback((owner: PieceOwner) => {
    if (pendingPromotion || selected?.from !== 'board') return;

    const movingPiece = selected.piece;
    const newBoard    = deepCopyBoard(board);
    newBoard[selected.row][selected.col] = null;
    setBoard(newBoard);

    const capturedType = (DEMOTE_MAP[movingPiece.type] ?? movingPiece.type) as PieceType;
    if (owner === 'sente') {
      setSenteCaptured(prev => addToCaptured(prev, capturedType));
    } else {
      setGoteCaptured(prev => addToCaptured(prev, capturedType));
    }
    setSelected(null);
  }, [board, selected, pendingPromotion]);

  const handleHandClick = useCallback((owner: PieceOwner, type: PieceType) => {
    if (pendingPromotion) return;

    if (!selected) {
      setSelected({ from: 'hand', owner, type });
      return;
    }
    if (selected.from === 'hand' && selected.owner === owner && selected.type === type) {
      setSelected(null);
      return;
    }
    if (selected.from === 'board') {
      handleHandZoneClick(owner);
      return;
    }
    setSelected({ from: 'hand', owner, type });
  }, [selected, pendingPromotion, handleHandZoneClick]);

  const confirmPromotion = useCallback((promote: boolean) => {
    if (!pendingPromotion) return;
    const { boardRow, boardCol, fromBoardRow, fromBoardCol, prePiece, owner } = pendingPromotion;

    if (promote) {
      const newBoard = deepCopyBoard(board);
      newBoard[boardRow][boardCol] = { type: PROMOTE_MAP[prePiece]!, owner };
      setBoard(newBoard);
    }

    const move: FreePlayMove = {
      kifRow: toKifRow(boardRow),
      kifCol: toKifCol(boardCol),
      fromKifRow: toKifRow(fromBoardRow),
      fromKifCol: toKifCol(fromBoardCol),
      prePiece,
      isDrop: false,
      isPromote: promote,
    };
    setMoveHistory(prev => [...prev, move]);
    setPendingPromotion(null);
  }, [board, pendingPromotion]);

  const isSelectedBoard = useCallback((row: number, col: number) =>
    selected?.from === 'board' && selected.row === row && selected.col === col,
  [selected]);

  const isSelectedHand = useCallback((owner: PieceOwner, type: PieceType) =>
    selected?.from === 'hand' && selected.owner === owner && selected.type === type,
  [selected]);

  const generateKIF = useCallback((originalKifText: string, originalMoveCount: number): string => {
    const lines = originalKifText.split('\n');
    const kept: string[] = [];
    for (const line of lines) {
      const m = /^\s*(\d+)\s+/.exec(line);
      if (m && parseInt(m[1], 10) > originalMoveCount) continue;
      kept.push(line);
    }
    while (kept.length > 0 && !kept[kept.length - 1].trim()) kept.pop();

    let prev: FreePlayMove | undefined;
    moveHistory.forEach((move, i) => {
      kept.push(formatMove(move, originalMoveCount + i + 1, prev));
      prev = move;
    });

    return kept.join('\n');
  }, [moveHistory]);

  const generateCurrentBOD = useCallback(() =>
    generateBOD(board, senteCaptured, goteCaptured),
  [board, senteCaptured, goteCaptured]);

  return {
    board,
    senteCaptured,
    goteCaptured,
    selected,
    moveHistory,
    pendingPromotion,
    handleBoardClick,
    handleHandClick,
    handleHandZoneClick,
    confirmPromotion,
    isSelectedBoard,
    isSelectedHand,
    initializeBoard,
    generateKIF,
    generateCurrentBOD,
  };
}
