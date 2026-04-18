'use client';

import { cn } from '@/lib/utils';
import type { BoardState, BoardSize } from '../types';
import { COLUMN_LABELS, ROW_LABELS } from '../constants';

interface BoardProps {
  board: BoardState;
  size?: BoardSize;
  className?: string;
  highlightCell?: { colIndex: number; rowIndex: number };
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { cellW: 'w-6', cellH: 'h-6', labelW: 'w-4', text: 'text-xs' };
    case 'lg': return { cellW: 'w-10', cellH: 'h-10', labelW: 'w-6', text: 'text-lg' };
    default:   return { cellW: 'w-8', cellH: 'h-8', labelW: 'w-5', text: 'text-sm' };
  }
}

export function Board({ board, size = 'md', className, highlightCell }: BoardProps) {
  const { cellW, cellH, labelW, text } = getSizeClasses(size);

  return (
    <div className={cn('inline-block', className)}>
      {/* 列ラベル */}
      <div className="flex">
        {COLUMN_LABELS.map((l) => (
          <div key={l} className={cn('flex items-center justify-center font-serif font-bold text-black', cellW, cellH, text)}>
            {l}
          </div>
        ))}
        <div className={labelW} />
      </div>

      <div className="flex items-stretch">
        {/* 盤面 */}
        <div className="relative border-2 border-black">
          {/* Layer 1: グリッド線 + ハイライト背景 */}
          <div className="grid grid-cols-9">
            {Array.from({ length: 81 }, (_, idx) => {
              const ci = idx % 9;
              const ri = Math.floor(idx / 9);
              return (
                <div
                  key={idx}
                  className={cn(
                    cellW, cellH,
                    ci < 8 && 'border-r border-gray-700',
                    ri < 8 && 'border-b border-gray-700',
                    'bg-white',
                  )}
                />
              );
            })}
          </div>

          {/* Layer 2: 駒の文字 */}
          <div className="absolute inset-0 grid grid-cols-9">
            {board.flat().map((piece, idx) => {
              const ci = idx % 9;
              const ri = Math.floor(idx / 9);
              const isHL = highlightCell?.colIndex === ci && highlightCell?.rowIndex === ri;
              return (
                <div key={idx} className={cn('flex items-center justify-center', cellW, cellH)}>
                  {piece && (
                    <span
                      className={cn(
                        'font-serif text-black select-none',
                        text,
                        piece.owner === 'gote' && 'rotate-180',
                        isHL ? 'font-black scale-115' : 'font-bold',
                      )}
                    >
                      {piece.type}
                    </span>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* 段ラベル */}
        <div className="flex flex-col">
          {ROW_LABELS.map((l) => (
            <div key={l} className={cn('flex items-center justify-center font-serif font-bold text-black', cellH, labelW, text)}>
              {l}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
