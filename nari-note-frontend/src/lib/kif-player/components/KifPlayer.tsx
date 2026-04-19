'use client';

import { useMemo, useState } from 'react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import type { BoardSize } from '../types';
import { Board } from './Board';
import { CapturedPieces } from './CapturedPieces';
import { parseKif } from '../utils/parseKif';
import { parseBOD } from '../utils/parseBOD';
import { getBoardAtMove } from '../utils/simulator';
import { ROW_LABELS } from '../constants';

interface KifPlayerProps {
  kifText: string;
  defaultMoveNumber?: number;
  moveNumber?: number;
  onMoveChange?: (move: number) => void;
  className?: string;
  size?: BoardSize;
  showCapturedPieces?: boolean;
  showPlayerNames?: boolean;
}

export function KifPlayer({
  kifText,
  defaultMoveNumber = 0,
  moveNumber,
  onMoveChange,
  className,
  size = 'md',
  showCapturedPieces = true,
  showPlayerNames = false,
}: KifPlayerProps) {
  const [internalMove, setInternalMove] = useState(defaultMoveNumber);
  const currentMove = moveNumber !== undefined ? moveNumber : internalMove;

  const setCurrentMove = (updater: number | ((prev: number) => number)) => {
    const next = typeof updater === 'function' ? updater(currentMove) : updater;
    if (moveNumber === undefined) setInternalMove(next);
    onMoveChange?.(next);
  };

  const isBOD = /^(後手の持駒|先手の持駒)：/.test(kifText.trim());

  const game = useMemo(() => {
    if (isBOD) return null;
    try {
      return parseKif(kifText);
    } catch {
      return null;
    }
  }, [kifText, isBOD]);

  const bodParsed = useMemo(() => {
    if (!isBOD) return null;
    try {
      return parseBOD(kifText);
    } catch {
      return null;
    }
  }, [kifText, isBOD]);

  const parsed = useMemo(() => {
    if (isBOD) return bodParsed;
    if (!game) return null;
    return getBoardAtMove(game, currentMove);
  }, [game, bodParsed, currentMove, isBOD]);

  if (!parsed) {
    return (
      <div className={cn('p-4 bg-red-50 border border-red-200 rounded', className)}>
        <p className="text-red-700 text-sm">棋譜の読み込みに失敗しました。</p>
      </div>
    );
  }

  const totalMoves = game?.moves.length ?? 0;

  return (
    <div className={cn('inline-flex flex-col items-center gap-2', className)}>
      {showCapturedPieces && (
        <CapturedPieces
          pieces={parsed.captured.gote}
          owner="gote"
          playerName={showPlayerNames ? parsed.gotePlayer : undefined}
          size={size}
        />
      )}
      <Board
        board={parsed.board}
        size={size}
        highlightCell={game && currentMove > 0 ? {
          colIndex: 9 - game.moves[currentMove - 1].toCol,
          rowIndex: game.moves[currentMove - 1].toRow - 1,
        } : undefined}
      />
      {showCapturedPieces && (
        <CapturedPieces
          pieces={parsed.captured.sente}
          owner="sente"
          playerName={showPlayerNames ? parsed.sentePlayer : undefined}
          size={size}
        />
      )}
      {!isBOD && (
      <div className="flex items-center gap-3 mt-1">
        <Button
          onClick={() => setCurrentMove((m) => Math.max(0, m - 1))}
          disabled={currentMove === 0}
          aria-label="前の手"
        >
          ←
        </Button>
        <span className="text-sm tabular-nums min-w-[10rem] text-center select-none">
          {currentMove === 0 ? '初期配置' : (() => {
            const m = game!.moves[currentMove - 1];
            const coord = m.isSameSquare ? '同' : `${m.toCol}${ROW_LABELS[m.toRow - 1]}`;
            return <>{currentMove}手目　{coord}<strong className="font-bold">{m.piece}</strong>{m.isPromote ? '成' : ''}</>;
          })()}
        </span>
        <Button
          onClick={() => setCurrentMove((m) => Math.min(totalMoves, m + 1))}
          disabled={currentMove === totalMoves}
          aria-label="次の手"
        >
          →
        </Button>
      </div>
      )}
    </div>
  );
}
