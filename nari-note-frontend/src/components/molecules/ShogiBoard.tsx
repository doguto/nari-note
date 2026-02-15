'use client';

import { KifuLite } from 'kifu-for-js';

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
 * kifu-for-js の静的図面モードを使用
 */
export function ShogiBoard({ bodText, className = '' }: ShogiBoardProps) {
  return (
    <div className={`my-4 ${className}`}>
      <KifuLite static>
        {bodText}
      </KifuLite>
    </div>
  );
}
