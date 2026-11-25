'use client';

import { ChevronLeft, ChevronRight, ChevronsLeft } from 'lucide-react';
import type { BoardSize } from '@/lib/kif-player';
import { Button } from '@/components/ui/button';
import { ResearchBoard } from './ResearchBoard';

interface BoardCardProps {
  sfen: string;
  legalMoves: string[];
  lastMoveUsi?: string | null;
  ply: number;
  lastMoveLabel?: string;
  size: BoardSize;
  disabled: boolean;
  canBack: boolean;
  canForward: boolean;
  onMove: (usi: string) => void;
  onFirst: () => void;
  onBack: () => void;
  onForward: () => void;
}

export function BoardCard({
  sfen,
  legalMoves,
  lastMoveUsi,
  ply,
  lastMoveLabel,
  size,
  disabled,
  canBack,
  canForward,
  onMove,
  onFirst,
  onBack,
  onForward,
}: BoardCardProps) {
  return (
    <section aria-label="盤面" className="flex flex-col gap-3 rounded-xl border border-brand-border bg-white px-3 py-3 shadow-sm sm:px-4">
      <div className="flex items-center gap-2 text-sm">
        <span className="text-brand-secondary-text">{ply === 0 ? '開始局面' : `${ply}手目`}</span>
        {lastMoveLabel && <b className="text-brand-text">{lastMoveLabel}</b>}
      </div>
      <div className="flex justify-center">
        <ResearchBoard
          key={sfen}
          sfen={sfen}
          legalMoves={legalMoves}
          lastMoveUsi={lastMoveUsi ?? undefined}
          size={size}
          disabled={disabled}
          onMove={onMove}
        />
      </div>
      <div className="flex justify-center gap-2">
        <Button variant="outline" size="icon" aria-label="最初へ" disabled={!canBack} onClick={onFirst}>
          <ChevronsLeft aria-hidden="true" />
        </Button>
        <Button variant="outline" size="icon" aria-label="1手戻る" disabled={!canBack} onClick={onBack}>
          <ChevronLeft aria-hidden="true" />
        </Button>
        <Button variant="outline" size="icon" aria-label="1手進む" disabled={!canForward} onClick={onForward}>
          <ChevronRight aria-hidden="true" />
        </Button>
      </div>
    </section>
  );
}
