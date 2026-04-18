'use client';

import { useMemo, useState } from 'react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import type { BoardSize } from '../types';
import { Board } from './Board';
import { CapturedPieces } from './CapturedPieces';
import { parseKif } from '../utils/parseKif';
import { getBoardAtMove } from '../utils/simulator';

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

  const game = useMemo(() => {
    try {
      return parseKif(kifText);
    } catch {
      return null;
    }
  }, [kifText]);

  const parsed = useMemo(() => {
    if (!game) return null;
    return getBoardAtMove(game, currentMove);
  }, [game, currentMove]);

  if (!game || !parsed) {
    return (
      <div className={cn('p-4 bg-red-50 border border-red-200 rounded', className)}>
        <p className="text-red-700 text-sm">棋譜の読み込みに失敗しました。</p>
      </div>
    );
  }

  const totalMoves = game.moves.length;

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
      <Board board={parsed.board} size={size} />
      {showCapturedPieces && (
        <CapturedPieces
          pieces={parsed.captured.sente}
          owner="sente"
          playerName={showPlayerNames ? parsed.sentePlayer : undefined}
          size={size}
        />
      )}
      <div className="flex items-center gap-3 mt-1">
        <Button
          onClick={() => setCurrentMove((m) => Math.max(0, m - 1))}
          disabled={currentMove === 0}
          aria-label="前の手"
        >
          ←
        </Button>
        <span className="text-sm tabular-nums min-w-[10rem] text-center select-none">
          {currentMove === 0 ? '初期配置' : `${currentMove}手目 / ${totalMoves}手中`}
        </span>
        <Button
          onClick={() => setCurrentMove((m) => Math.min(totalMoves, m + 1))}
          disabled={currentMove === totalMoves}
          aria-label="次の手"
        >
          →
        </Button>
      </div>
    </div>
  );
}
