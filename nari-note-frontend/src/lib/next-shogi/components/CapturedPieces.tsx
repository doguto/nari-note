'use client';

import type { CapturedPiece, PieceOwner, BoardSize } from '../types';
import { cn } from '@/lib/utils';

interface CapturedPiecesProps {
  pieces: CapturedPiece[];
  owner: PieceOwner;
  playerName?: string;
  size?: BoardSize;
}

/**
 * サイズに応じたテキストクラスを返す
 */
function getSizeClass(size: BoardSize = 'md'): string {
  switch (size) {
    case 'sm':
      return 'text-xs';
    case 'lg':
      return 'text-lg';
    case 'md':
    default:
      return 'text-sm';
  }
}

/**
 * 持駒を表示するコンポーネント
 */
export function CapturedPieces({
  pieces,
  owner,
  playerName,
  size = 'md',
}: CapturedPiecesProps) {
  const sizeClass = getSizeClass(size);
  const label = owner === 'sente' ? '先手' : '後手';
  const displayName = playerName || label;

  return (
    <div className={cn('flex items-center gap-2 py-2 font-serif', sizeClass)}>
      <span className="font-bold text-black">{displayName}の持駒：</span>
      {pieces.length === 0 ? (
        <span className="text-gray-600">なし</span>
      ) : (
        <div className="flex flex-wrap gap-2">
          {pieces.map((piece, index) => (
            <span key={index} className="text-black">
              {piece.type}
              {piece.count > 1 && (
                <span className="ml-0.5 text-gray-600">×{piece.count}</span>
              )}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
