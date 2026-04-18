'use client';

import { useMemo } from 'react';
import { cn } from '@/lib/utils';
import type { BoardSize } from '../types';
import { parseBOD } from '../utils/parseBOD';
import { Board } from './Board';
import { CapturedPieces } from './CapturedPieces';

interface ShogiBoardProps {
  bodText: string;
  className?: string;
  size?: BoardSize;
  showCapturedPieces?: boolean;
  showPlayerNames?: boolean;
}

export function ShogiBoard({
  bodText,
  className,
  size = 'md',
  showCapturedPieces = true,
  showPlayerNames = true,
}: ShogiBoardProps) {
  const parsed = useMemo(() => {
    try {
      return parseBOD(bodText);
    } catch {
      return null;
    }
  }, [bodText]);

  if (!parsed) {
    return (
      <div className={cn('p-4 bg-red-50 border border-red-200 rounded', className)}>
        <p className="text-red-700 text-sm">将棋盤面の読み込みに失敗しました。BOD形式が正しいか確認してください。</p>
      </div>
    );
  }

  return (
    <div className={className}>
      {showCapturedPieces && (
        <CapturedPieces pieces={parsed.captured.gote} owner="gote" playerName={showPlayerNames ? parsed.gotePlayer : undefined} size={size} />
      )}
      <Board board={parsed.board} size={size} />
      {showCapturedPieces && (
        <CapturedPieces pieces={parsed.captured.sente} owner="sente" playerName={showPlayerNames ? parsed.sentePlayer : undefined} size={size} />
      )}
    </div>
  );
}
