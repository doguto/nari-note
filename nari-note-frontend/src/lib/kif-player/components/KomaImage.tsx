'use client';

import { cn } from '@/lib/utils';
import { KOMA_LABEL_MAP, KOMA_SHAPE_POINTS, PROMOTED_PIECES } from '../constants';
import type { PieceOwner, PieceType } from '../types';

interface KomaImageProps {
  type: PieceType;
  owner: PieceOwner;
  size: number;
  className?: string;
  /** 後手の駒を180度回転させるか（盤上表示では true、持ち駒表示では false） */
  rotate?: boolean;
}

export function KomaImage({ type, owner, size, className, rotate = true }: KomaImageProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 60 60"
      role="img"
      aria-label={type}
      className={cn('select-none pointer-events-none font-koma', rotate && owner === 'gote' && 'rotate-180', className)}
    >
      <polygon
        points={KOMA_SHAPE_POINTS}
        className="fill-shogi-koma stroke-shogi-koma-edge"
        strokeWidth={2.5}
        strokeLinejoin="round"
      />
      <text
        x={30}
        y={35}
        textAnchor="middle"
        dominantBaseline="central"
        fontSize={30}
        fontWeight={700}
        className={PROMOTED_PIECES.has(type) ? 'fill-shogi-promoted' : 'fill-shogi-ink'}
      >
        {KOMA_LABEL_MAP[type]}
      </text>
    </svg>
  );
}
