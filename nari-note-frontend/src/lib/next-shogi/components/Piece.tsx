'use client';

import type { Piece as PieceType, BoardSize } from '../types';
import { cn } from '@/lib/utils';

interface PieceProps {
  piece: PieceType;
  size?: BoardSize;
}

/**
 * 駒のサイズに応じたクラスを返す
 */
function getSizeClasses(size: BoardSize = 'md'): {
  container: string;
  text: string;
} {
  switch (size) {
    case 'sm':
      return {
        container: 'w-6 h-7',
        text: 'text-xs',
      };
    case 'lg':
      return {
        container: 'w-10 h-12',
        text: 'text-lg',
      };
    case 'md':
    default:
      return {
        container: 'w-8 h-9',
        text: 'text-sm',
      };
  }
}

/**
 * 駒を表示するコンポーネント
 */
export function Piece({ piece, size = 'md' }: PieceProps) {
  const sizeClasses = getSizeClasses(size);

  return (
    <div
      className={cn(
        'flex items-center justify-center',
        'border border-gray-700 bg-white',
        'font-serif font-bold text-black',
        sizeClasses.container,
        sizeClasses.text,
        // 後手の駒は上下反転
        piece.owner === 'gote' && 'rotate-180'
      )}
    >
      {piece.type}
    </div>
  );
}
