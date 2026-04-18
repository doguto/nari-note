'use client';

import { cn } from '@/lib/utils';
import type { Piece as PieceData, BoardSize } from '../types';

interface PieceProps {
  piece: PieceData;
  size?: BoardSize;
  isHighlighted?: boolean;
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { container: 'w-6 h-7', text: 'text-xs' };
    case 'lg': return { container: 'w-10 h-12', text: 'text-lg' };
    default:   return { container: 'w-8 h-9',  text: 'text-sm' };
  }
}

export function Piece({ piece, size = 'md', isHighlighted = false }: PieceProps) {
  const { container, text } = getSizeClasses(size);
  return (
    <div
      className={cn(
        'flex items-center justify-center',
        'border border-gray-700 bg-white',
        container,
        piece.owner === 'gote' && 'rotate-180'
      )}
    >
      <span
        className={cn(
          'font-serif text-black',
          text,
          isHighlighted ? 'font-black scale-115' : 'font-bold',
        )}
      >
        {piece.type}
      </span>
    </div>
  );
}
