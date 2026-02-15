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

// kifu-for-js の KifuLite コンポーネントの型定義
type KifuLiteComponent = React.ComponentType<{
  kifu?: string;
  ply?: number;
  static?: { last?: 'hidden' | [number, number] };
}>;

/**
 * ShogiBoard - Molecule Component
 *
 * BOD形式の将棋盤面をレンダリングする汎用コンポーネント
 * kifu-for-js の静的図面モードを使用
 */
export function ShogiBoard({ bodText, className = '' }: ShogiBoardProps) {
  const [KifuLite, setKifuLite] = useState<KifuLiteComponent | null>(null);
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);

    // クライアントサイドでのみ kifu-for-js をインポート
    import('kifu-for-js')
      .then((module) => {
        if (module.KifuLite) {
          setKifuLite(() => module.KifuLite);
        } else {
          console.error('KifuLite component not found in kifu-for-js module');
        }
      })
      .catch((error) => {
        console.error('Failed to load kifu-for-js:', error);
      });
  }, []);

  // サーバーサイドレンダリング時とクライアント初期マウント時の表示を統一
  if (!isMounted || !KifuLite) {
    return (
      <div className={`my-4 p-4 bg-gray-100 rounded ${className}`}>
        <div className="h-64 bg-gray-200 rounded animate-pulse"></div>
      </div>
    );
  }

  return (
    <div className={`my-4 ${className}`}>
      <KifuLite
        kifu={bodText}
        static={{ last: 'hidden' }}
      />
    </div>
  );
}
