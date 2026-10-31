'use client';

import { cn } from '@/lib/utils';
import { KOMA_SHAPE_POINTS, PROMOTED_PIECES } from '../constants';
import { KOMA_GLYPH_PATHS } from '../constants/komaGlyphPaths';
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
      className={cn('select-none pointer-events-none', rotate && owner === 'gote' && 'rotate-180', className)}
    >
      <polygon
        points={KOMA_SHAPE_POINTS}
        className="fill-shogi-koma stroke-shogi-koma-edge"
        strokeWidth={2.5}
        strokeLinejoin="round"
      />
      {/* 文字はフォントの読み込みを待たずに表示できるよう、アウトライン化したパスで描く */}
      <path
        d={KOMA_GLYPH_PATHS[type]}
        className={PROMOTED_PIECES.has(type) ? 'fill-shogi-promoted' : 'fill-shogi-ink'}
      />
    </svg>
  );
}
