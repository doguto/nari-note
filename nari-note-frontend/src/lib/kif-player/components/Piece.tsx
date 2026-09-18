'use client';

import { cn } from '@/lib/utils';
import type { Piece as PieceData, BoardSize } from '../types';
import { KomaImage } from './KomaImage';

interface PieceProps {
  piece: PieceData;
  size?: BoardSize;
  isHighlighted?: boolean;
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { container: 'w-6 h-7', imageSize: 22 };
    case 'lg': return { container: 'w-10 h-12', imageSize: 36 };
    default:   return { container: 'w-8 h-9',  imageSize: 28 };
  }
}

export function Piece({ piece, size = 'md', isHighlighted = false }: PieceProps) {
  const { container, imageSize } = getSizeClasses(size);
  return (
    <div
      className={cn(
        'flex items-center justify-center',
        'border border-gray-700 bg-shogi-board',
        container,
        isHighlighted && 'scale-115',
      )}
    >
      <KomaImage type={piece.type} owner={piece.owner} size={imageSize} />
    </div>
  );
}
