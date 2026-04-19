'use client';

import { useState, useCallback } from 'react';
import { INITIAL_BOARD } from '../constants';
import {
  DEMOTE_MAP,
  deepCopyBoard,
  addToCaptured,
  removeFromCaptured,
  generateBOD,
} from '../utils/boardEditor';
import type { BoardState, CapturedPiece, Piece, PieceOwner, PieceType } from '../types';

export type BoardEditorSelection =
  | { from: 'board'; row: number; col: number; piece: Piece }
  | { from: 'hand';  owner: PieceOwner; type: PieceType };

export interface BoardEditorState {
  board: BoardState;
  senteCaptured: CapturedPiece[];
  goteCaptured: CapturedPiece[];
  selected: BoardEditorSelection | null;
  handleBoardClick: (row: number, col: number) => void;
  handleHandClick: (owner: PieceOwner, type: PieceType) => void;
  handleHandZoneClick: (owner: PieceOwner) => void;
  resetBoard: () => void;
  clearBoard: () => void;
  isSelectedBoard: (row: number, col: number) => boolean;
  isSelectedHand: (owner: PieceOwner, type: PieceType) => boolean;
  generateCurrentBOD: () => string;
  initializeBoard: (board: BoardState, sente: CapturedPiece[], gote: CapturedPiece[]) => void;
}

export function useBoardEditor(): BoardEditorState {
  const [board, setBoard]                 = useState<BoardState>(() => deepCopyBoard(INITIAL_BOARD));
  const [senteCaptured, setSenteCaptured] = useState<CapturedPiece[]>([]);
  const [goteCaptured,  setGoteCaptured]  = useState<CapturedPiece[]>([]);
  const [selected, setSelected]           = useState<BoardEditorSelection | null>(null);

  const resetBoard = useCallback(() => {
    setBoard(deepCopyBoard(INITIAL_BOARD));
    setSenteCaptured([]);
    setGoteCaptured([]);
    setSelected(null);
  }, []);

  const clearBoard = useCallback(() => {
    let newSente: CapturedPiece[] = [...senteCaptured];
    let newGote:  CapturedPiece[] = [...goteCaptured];

    for (let r = 0; r < 9; r++) {
      for (let c = 0; c < 9; c++) {
        const piece = board[r][c];
        if (!piece) continue;
        const capturedType = (DEMOTE_MAP[piece.type] ?? piece.type) as PieceType;
        if (piece.owner === 'sente') {
          newSente = addToCaptured(newSente, capturedType);
        } else {
          newGote = addToCaptured(newGote, capturedType);
        }
      }
    }

    setBoard(Array.from({ length: 9 }, () => Array.from({ length: 9 }, () => null)));
    setSenteCaptured(newSente);
    setGoteCaptured(newGote);
    setSelected(null);
  }, [board, senteCaptured, goteCaptured]);

  const handleBoardClick = useCallback((row: number, col: number) => {
    const piece = board[row][col];

    if (!selected) {
      if (piece) setSelected({ from: 'board', row, col, piece });
      return;
    }

    if (selected.from === 'board' && selected.row === row && selected.col === col) {
      setSelected(null);
      return;
    }

    const newBoard = deepCopyBoard(board);

    if (selected.from === 'board') {
      const movingPiece = selected.piece;
      const displaced   = newBoard[row][col];
      newBoard[selected.row][selected.col] = null;
      newBoard[row][col] = movingPiece;

      if (displaced) {
        const capturedType = (DEMOTE_MAP[displaced.type] ?? displaced.type) as PieceType;
        if (movingPiece.owner === 'sente') {
          setSenteCaptured(prev => addToCaptured(prev, capturedType));
        } else {
          setGoteCaptured(prev => addToCaptured(prev, capturedType));
        }
      }
    } else {
      // 持ち駒を盤面に打つ
      const displaced    = newBoard[row][col];
      newBoard[row][col] = { type: selected.type, owner: selected.owner };

      if (selected.owner === 'sente') {
        setSenteCaptured(prev => removeFromCaptured(prev, selected.type));
      } else {
        setGoteCaptured(prev => removeFromCaptured(prev, selected.type));
      }

      if (displaced) {
        const capturedType = (DEMOTE_MAP[displaced.type] ?? displaced.type) as PieceType;
        if (selected.owner === 'sente') {
          setSenteCaptured(prev => addToCaptured(prev, capturedType));
        } else {
          setGoteCaptured(prev => addToCaptured(prev, capturedType));
        }
      }
    }

    setBoard(newBoard);
    setSelected(null);
  }, [board, selected]);

  const handleHandZoneClick = useCallback((owner: PieceOwner) => {
    if (selected?.from !== 'board') return;

    const movingPiece  = selected.piece;
    const newBoard     = deepCopyBoard(board);
    newBoard[selected.row][selected.col] = null;
    setBoard(newBoard);

    const capturedType = (DEMOTE_MAP[movingPiece.type] ?? movingPiece.type) as PieceType;
    if (owner === 'sente') {
      setSenteCaptured(prev => addToCaptured(prev, capturedType));
    } else {
      setGoteCaptured(prev => addToCaptured(prev, capturedType));
    }
    setSelected(null);
  }, [board, selected]);

  const handleHandClick = useCallback((owner: PieceOwner, type: PieceType) => {
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
  }, [selected, handleHandZoneClick]);

  const isSelectedBoard = useCallback((row: number, col: number) =>
    selected?.from === 'board' && selected.row === row && selected.col === col,
  [selected]);

  const isSelectedHand = useCallback((owner: PieceOwner, type: PieceType) =>
    selected?.from === 'hand' && selected.owner === owner && selected.type === type,
  [selected]);

  const generateCurrentBOD = useCallback(() =>
    generateBOD(board, senteCaptured, goteCaptured),
  [board, senteCaptured, goteCaptured]);

  const initializeBoard = useCallback((newBoard: BoardState, sente: CapturedPiece[], gote: CapturedPiece[]) => {
    setBoard(deepCopyBoard(newBoard));
    setSenteCaptured([...sente.map(p => ({ ...p }))]);
    setGoteCaptured([...gote.map(p => ({ ...p }))]);
    setSelected(null);
  }, []);

  return {
    board,
    senteCaptured,
    goteCaptured,
    selected,
    handleBoardClick,
    handleHandClick,
    handleHandZoneClick,
    resetBoard,
    clearBoard,
    isSelectedBoard,
    isSelectedHand,
    generateCurrentBOD,
    initializeBoard,
  };
}
