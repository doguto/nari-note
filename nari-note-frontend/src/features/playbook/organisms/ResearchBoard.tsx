'use client';

import { useMemo, useState } from 'react';
import { Board, CapturedPieces } from '@/lib/kif-player';
import type { BoardSize, PieceOwner, PieceType } from '@/lib/kif-player';
import { Button } from '@/components/ui/button';
import { cellOfUsiSquare, dropLetter, parseSfen, usiSquare } from '../utils/sfen';

type Selection = { kind: 'board'; square: string; row: number; col: number } | { kind: 'hand'; letter: string; type: PieceType };

interface ResearchBoardProps {
  sfen: string;
  legalMoves: string[];
  lastMoveUsi?: string;
  size?: BoardSize;
  disabled?: boolean;
  onMove: (usi: string) => void;
}

// 盤上の駒か持駒を選び、移動先を押すと手を指す。成れる手は成るかどうかを選ばせる
export function ResearchBoard({ sfen, legalMoves, lastMoveUsi, size = 'lg', disabled = false, onMove }: ResearchBoardProps) {
  const parsed = useMemo(() => parseSfen(sfen), [sfen]);
  const [selection, setSelection] = useState<Selection | null>(null);
  const [promotionChoices, setPromotionChoices] = useState<string[] | null>(null);

  const candidates = useMemo(() => {
    if (!selection) return [];
    if (selection.kind === 'hand') return legalMoves.filter((m) => m.startsWith(`${selection.letter}*`));
    return legalMoves.filter((m) => m[1] !== '*' && m.startsWith(selection.square));
  }, [selection, legalMoves]);

  const markedCells = useMemo(
    () => [...new Set(candidates.map((m) => m.slice(2, 4)))].map(cellOfUsiSquare),
    [candidates],
  );

  const play = (usi: string) => {
    setSelection(null);
    setPromotionChoices(null);
    onMove(usi);
  };

  const handleCellClick = (row: number, col: number) => {
    if (disabled || promotionChoices) return;
    const square = usiSquare(row, col);
    const piece = parsed.board[row]?.[col];
    const isOwn = piece?.owner === parsed.turn;

    if (selection) {
      const moves = candidates.filter((m) => m.slice(2, 4) === square);
      if (moves.length === 1) return play(moves[0]);
      if (moves.length > 1) return setPromotionChoices(moves);
    }
    if (isOwn && !(selection?.kind === 'board' && selection.square === square)) {
      return setSelection({ kind: 'board', square, row, col });
    }
    setSelection(null);
  };

  const handleHandClick = (owner: PieceOwner, type: PieceType) => {
    if (disabled || promotionChoices || owner !== parsed.turn) return;
    const letter = dropLetter(type);
    if (!letter) return;
    setSelection(selection?.kind === 'hand' && selection.letter === letter ? null : { kind: 'hand', letter, type });
  };

  const isHandSelected = (owner: PieceOwner, type: PieceType) =>
    selection?.kind === 'hand' && owner === parsed.turn && selection.type === type;

  const lastTo = lastMoveUsi ? cellOfUsiSquare(lastMoveUsi.slice(2, 4)) : undefined;

  return (
    <div className="relative inline-flex flex-col">
      <CapturedPieces
        pieces={parsed.captured.gote}
        owner="gote"
        size={size}
        onPieceClick={handleHandClick}
        isSelected={isHandSelected}
      />
      <Board
        board={parsed.board}
        size={size}
        highlightCell={lastTo ? { colIndex: lastTo.col, rowIndex: lastTo.row } : undefined}
        selectedCell={selection?.kind === 'board' ? { row: selection.row, col: selection.col } : null}
        markedCells={markedCells}
        onCellClick={handleCellClick}
      />
      <CapturedPieces
        pieces={parsed.captured.sente}
        owner="sente"
        size={size}
        onPieceClick={handleHandClick}
        isSelected={isHandSelected}
      />
      {promotionChoices && (
        <div className="absolute inset-x-4 top-1/3 flex items-center justify-center gap-3 rounded-lg border border-brand-border bg-white p-4 shadow-lg">
          <span className="text-sm text-gray-700">成りますか？</span>
          <Button
            className="bg-brand-primary-dark hover:bg-brand-text text-white"
            onClick={() => play(promotionChoices.find((m) => m.endsWith('+'))!)}
          >
            成る
          </Button>
          <Button variant="outline" onClick={() => play(promotionChoices.find((m) => !m.endsWith('+'))!)}>
            成らない
          </Button>
        </div>
      )}
    </div>
  );
}
