'use client';

import { cn } from '@/lib/utils';
import type { BoardState, BoardSize } from '../types';
import { COLUMN_LABELS, ROW_LABELS } from '../constants';
import { Piece } from './Piece';

interface BoardProps {
  board: BoardState;
  size?: BoardSize;
  className?: string;
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { cell: 'w-6 h-7', label: 'text-xs w-4' };
    case 'lg': return { cell: 'w-10 h-12', label: 'text-lg w-6' };
    default:   return { cell: 'w-8 h-9',  label: 'text-sm w-5' };
  }
}

export function Board({ board, size = 'md', className }: BoardProps) {
  const { cell, label } = getSizeClasses(size);
  return (
    <div className={cn('inline-block', className)}>
      <div className="flex">
        {COLUMN_LABELS.map((l) => (
          <div key={l} className={cn('flex items-center justify-center font-serif font-bold text-black', cell)}>
            {l}
          </div>
        ))}
        <div className={label} />
      </div>
      <div className="border-2 border-black">
        {board.map((row, rowIndex) => (
          <div key={rowIndex} className="flex">
            {row.map((piece, colIndex) => (
              <div
                key={colIndex}
                className={cn(
                  'relative flex items-center justify-center bg-white',
                  cell,
                  colIndex < 8 && 'border-r border-gray-700',
                  rowIndex < 8 && 'border-b border-gray-700'
                )}
              >
                {piece && <Piece piece={piece} size={size} />}
              </div>
            ))}
            <div className={cn('flex items-center justify-center font-serif font-bold text-black', label)}>
              {ROW_LABELS[rowIndex]}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
