'use client';

import type { BoardState, BoardSize } from '../types';
import { COLUMN_LABELS, ROW_LABELS } from '../constants';
import { Piece } from './Piece';
import { cn } from '@/lib/utils';

interface BoardProps {
  board: BoardState;
  size?: BoardSize;
  className?: string;
}

/**
 * サイズに応じたクラスを返す
 */
function getSizeClasses(size: BoardSize = 'md'): {
  cell: string;
  label: string;
} {
  switch (size) {
    case 'sm':
      return {
        cell: 'w-6 h-7',
        label: 'text-xs w-4',
      };
    case 'lg':
      return {
        cell: 'w-10 h-12',
        label: 'text-lg w-6',
      };
    case 'md':
    default:
      return {
        cell: 'w-8 h-9',
        label: 'text-sm w-5',
      };
  }
}

/**
 * 将棋盤面を表示するコンポーネント
 */
export function Board({ board, size = 'md', className }: BoardProps) {
  const sizeClasses = getSizeClasses(size);

  return (
    <div className={cn('inline-block', className)}>
      {/* 列番号（9〜1）- 上のみ */}
      <div className="flex">
        {COLUMN_LABELS.map((label) => (
          <div
            key={label}
            className={cn(
              'flex items-center justify-center font-serif font-bold text-black',
              sizeClasses.cell
            )}
          >
            {label}
          </div>
        ))}
        <div className={sizeClasses.label}></div>
      </div>

      {/* 盤面 */}
      <div className="border-2 border-black">
        {board.map((row, rowIndex) => (
          <div key={rowIndex} className="flex">
            {/* マス */}
            {row.map((piece, colIndex) => (
              <div
                key={colIndex}
                className={cn(
                  'relative flex items-center justify-center bg-white',
                  sizeClasses.cell,
                  // マスの境界線
                  colIndex < 8 && 'border-r border-gray-700',
                  rowIndex < 8 && 'border-b border-gray-700'
                )}
              >
                {piece && <Piece piece={piece} size={size} />}
              </div>
            ))}

            {/* 右側の行番号のみ */}
            <div
              className={cn(
                'flex items-center justify-center font-serif font-bold text-black',
                sizeClasses.label
              )}
            >
              {ROW_LABELS[rowIndex]}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
