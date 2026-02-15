'use client';

import { useEffect, useState } from 'react';

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
  const [KifuLite, setKifuLite] = useState<any>(null);
  
  useEffect(() => {
    // クライアントサイドでのみ kifu-for-js をインポート
    import('kifu-for-js').then((module) => {
      setKifuLite(() => module.KifuLite);
    });
  }, []);
  
  if (!KifuLite) {
    return (
      <div className={`my-4 p-4 bg-gray-100 rounded ${className}`}>
        <pre className="whitespace-pre-wrap font-mono text-sm">{bodText}</pre>
      </div>
    );
  }
  
  return (
    <div className={`my-4 ${className}`}>
      <KifuLite static>
        {bodText}
      </KifuLite>
    </div>
  );
}
