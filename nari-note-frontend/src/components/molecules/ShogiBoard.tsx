'use client';

import { ShogiBoard as NextShogiBoard } from '@/lib/next-shogi';

interface ShogiBoardProps {
  /**
   * BOD形式の将棋盤面テキスト
   */
  bodText: string;
  /**
   * 追加のクラス名
   */
  className?: string;
}

/**
 * ShogiBoard - Molecule Component
 *
 * BOD形式の将棋盤面をレンダリングする汎用コンポーネント
 * next-shogi ライブラリを使用
 */
export function ShogiBoard({ bodText, className = '' }: ShogiBoardProps) {
  return <NextShogiBoard bodText={bodText} className={`my-4 ${className}`} />;
}
