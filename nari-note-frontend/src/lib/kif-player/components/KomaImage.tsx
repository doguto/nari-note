'use client';

import Image from 'next/image';
import { cn } from '@/lib/utils';
import { KOMA_IMAGE_MAP } from '../constants';
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
    <Image
      src={`/koma/${KOMA_IMAGE_MAP[type]}.png`}
      alt={type}
      width={size}
      height={size}
      draggable={false}
      className={cn('select-none pointer-events-none', rotate && owner === 'gote' && 'rotate-180', className)}
    />
  );
}
