'use client';

import { cn } from '@/lib/utils';
import type { CapturedPiece, PieceOwner, BoardSize, PieceType } from '../types';
import { KomaImage } from './KomaImage';

interface CapturedPiecesProps {
  pieces: CapturedPiece[];
  owner: PieceOwner;
  playerName?: string;
  size?: BoardSize;
  onPieceClick?: (owner: PieceOwner, type: PieceType) => void;
  isSelected?: (owner: PieceOwner, type: PieceType) => boolean;
  onHandZoneClick?: (owner: PieceOwner) => void;
}

function getSizeClasses(size: BoardSize = 'md') {
  switch (size) {
    case 'sm': return { text: 'text-xs', pieceSize: 18 };
    case 'lg': return { text: 'text-lg', pieceSize: 28 };
    default:   return { text: 'text-sm', pieceSize: 22 };
  }
}

export function CapturedPieces({
  pieces, owner, playerName, size = 'md',
  onPieceClick, isSelected, onHandZoneClick,
}: CapturedPiecesProps) {
  const { text, pieceSize } = getSizeClasses(size);
  const displayName = playerName || (owner === 'sente' ? '先手' : '後手');
  const interactive = !!onPieceClick;

  return (
    <div
      className={cn('flex items-center gap-2 py-2 font-serif', text)}
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
                  'inline-flex items-center',
                  interactive && 'cursor-pointer rounded px-0.5',
                  sel && 'bg-yellow-300',
                )}
                onClick={(e) => {
                  if (!interactive) return;
                  e.stopPropagation();
                  onPieceClick!(owner, piece.type);
                }}
              >
                <KomaImage type={piece.type} owner={owner} size={pieceSize} rotate={false} />
                {piece.count > 1 && <span className="ml-0.5 text-gray-600">×{piece.count}</span>}
              </span>
            );
          })}
        </div>
      )}
    </div>
  );
}
