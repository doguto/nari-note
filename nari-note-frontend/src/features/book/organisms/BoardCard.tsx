'use client';

import { ArrowDown, ArrowLeft, ArrowRight, ArrowUp, ChevronLeft, ChevronRight, ChevronsLeft } from 'lucide-react';
import type { ReactNode } from 'react';
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
  showKeyboardHint: boolean;
  onMove: (usi: string) => void;
  onFirst: () => void;
  onBack: () => void;
  onForward: () => void;
}

function Key({ children }: { children: ReactNode }) {
  return (
    <kbd className="inline-flex h-5 min-w-5 items-center justify-center rounded border border-b-2 border-brand-border bg-brand-bg-light px-1 font-sans text-[11px] text-brand-text">
      {children}
    </kbd>
  );
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
  showKeyboardHint,
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
      {showKeyboardHint && (
        <div aria-label="キーボード操作" className="flex flex-wrap justify-center gap-x-4 gap-y-1 text-xs text-brand-secondary-text">
          <span className="inline-flex items-center gap-1"><Key><ArrowLeft className="h-3 w-3" aria-label="左" /></Key><Key><ArrowRight className="h-3 w-3" aria-label="右" /></Key>1手</span>
          <span className="inline-flex items-center gap-1"><Key><ArrowUp className="h-3 w-3" aria-label="上" /></Key><Key><ArrowDown className="h-3 w-3" aria-label="下" /></Key>進む手を選ぶ</span>
        </div>
      )}
    </section>
  );
}
