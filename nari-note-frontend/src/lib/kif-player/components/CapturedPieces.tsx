'use client';

import { cn } from '@/lib/utils';
import type { CapturedPiece, PieceOwner, BoardSize, PieceType } from '../types';

interface CapturedPiecesProps {
  pieces: CapturedPiece[];
  owner: PieceOwner;
  playerName?: string;
  size?: BoardSize;
  onPieceClick?: (owner: PieceOwner, type: PieceType) => void;
  isSelected?: (owner: PieceOwner, type: PieceType) => boolean;
  onHandZoneClick?: (owner: PieceOwner) => void;
}

function getSizeClass(size: BoardSize = 'md'): string {
  switch (size) {
    case 'sm': return 'text-xs';
    case 'lg': return 'text-lg';
    default:   return 'text-sm';
  }
}

export function CapturedPieces({
  pieces, owner, playerName, size = 'md',
  onPieceClick, isSelected, onHandZoneClick,
}: CapturedPiecesProps) {
  const sizeClass = getSizeClass(size);
  const displayName = playerName || (owner === 'sente' ? '先手' : '後手');
  const interactive = !!onPieceClick;

  return (
    <div
      className={cn('flex items-center gap-2 py-2 font-serif', sizeClass)}
      onClick={() => onHandZoneClick?.(owner)}
    >
      <span className="font-bold text-black">{displayName}の持駒：</span>
      {pieces.length === 0 ? (
        <span className="text-gray-600">なし</span>
      ) : (
        <div className="flex flex-wrap gap-2">
          {pieces.map((piece, index) => {
            const sel = isSelected?.(owner, piece.type);
            return (
              <span
                key={index}
                className={cn(
                  'text-black',
                  interactive && 'cursor-pointer rounded px-0.5',
                  sel && 'bg-yellow-300',
                )}
                onClick={(e) => {
                  if (!interactive) return;
                  e.stopPropagation();
                  onPieceClick!(owner, piece.type);
                }}
              >
                {piece.type}
                {piece.count > 1 && <span className="ml-0.5 text-gray-600">×{piece.count}</span>}
              </span>
            );
          })}
        </div>
      )}
    </div>
  );
}
