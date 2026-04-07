'use client';

import { useMemo } from 'react';
import type { BoardSize } from '../types';
import { parseBOD } from '../utils';
import { Board } from './Board';
import { CapturedPieces } from './CapturedPieces';
import { cn } from '@/lib/utils';

interface ShogiBoardProps {
  /**
   * BOD形式の将棋盤面テキスト
   */
  bodText: string;
  /**
   * 追加のクラス名
   */
  className?: string;
  /**
   * 盤面のサイズ
   * @default 'md'
   */
  size?: BoardSize;
  /**
   * 持駒を表示するか
   * @default true
   */
  showCapturedPieces?: boolean;
  /**
   * プレイヤー名を表示するか
   * @default true
   */
  showPlayerNames?: boolean;
}

/**
 * BOD形式の将棋盤面を表示するメインコンポーネント
 *
 * @example
 * ```tsx
 * <ShogiBoard
 *   bodText={bodString}
 *   size="md"
 *   showCapturedPieces={true}
 *   showPlayerNames={true}
 * />
 * ```
 */
export function ShogiBoard({
  bodText,
  className,
  size = 'md',
  showCapturedPieces = true,
  showPlayerNames = true,
}: ShogiBoardProps) {
  // BOD形式をパース（メモ化）
  const parsed = useMemo(() => {
    try {
      return parseBOD(bodText);
    } catch (error) {
      console.error('Failed to parse BOD text:', error);
      return null;
    }
  }, [bodText]);

  // パースに失敗した場合のエラー表示
  if (!parsed) {
    return (
      <div className={cn('p-4 bg-red-50 border border-red-200 rounded', className)}>
        <p className="text-red-700 text-sm">
          将棋盤面の読み込みに失敗しました。BOD形式が正しいか確認してください。
        </p>
      </div>
    );
  }

  return (
    <div className={className}>
      {/* 後手の持駒 */}
      {showCapturedPieces && (
        <CapturedPieces
          pieces={parsed.captured.gote}
          owner="gote"
          playerName={showPlayerNames ? parsed.gotePlayer : undefined}
          size={size}
        />
      )}

      {/* 盤面 */}
      <Board board={parsed.board} size={size} />

      {/* 先手の持駒 */}
      {showCapturedPieces && (
        <CapturedPieces
          pieces={parsed.captured.sente}
          owner="sente"
          playerName={showPlayerNames ? parsed.sentePlayer : undefined}
          size={size}
        />
      )}
    </div>
  );
}
