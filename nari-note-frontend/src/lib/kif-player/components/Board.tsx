'use client';

import { cn } from '@/lib/utils';
import type { BoardState, BoardSize } from '../types';
import { COLUMN_LABELS, ROW_LABELS } from '../constants';
import { KomaImage } from './KomaImage';

interface BoardProps {
  board: BoardState;
  size?: BoardSize;
  className?: string;
  highlightCell?: { colIndex: number; rowIndex: number };
  selectedCell?: { row: number; col: number } | null;
  onCellClick?: (row: number, col: number) => void;
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { cellW: 'w-6', cellH: 'h-6', labelW: 'w-4', text: 'text-xs', pieceSize: 20 };
    case 'lg': return { cellW: 'w-10', cellH: 'h-10', labelW: 'w-6', text: 'text-lg', pieceSize: 36 };
    default:   return { cellW: 'w-8', cellH: 'h-8', labelW: 'w-5', text: 'text-sm', pieceSize: 28 };
  }
}

export function Board({ board, size = 'md', className, highlightCell, selectedCell, onCellClick }: BoardProps) {
  const { cellW, cellH, labelW, text, pieceSize } = getSizeClasses(size);

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
                    'bg-shogi-board',
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
              const isSel = selectedCell?.row === ri && selectedCell?.col === ci;
              return (
                <div
                  key={idx}
                  className={cn(
                    'flex items-center justify-center',
                    cellW, cellH,
                    onCellClick && 'cursor-pointer',
                    isSel && 'bg-yellow-300/70',
                  )}
                  onClick={() => onCellClick?.(ri, ci)}
                >
                  {piece && (
                    <KomaImage
                      type={piece.type}
                      owner={piece.owner}
                      size={pieceSize}
                      className={isHL ? 'scale-115' : undefined}
                    />
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
