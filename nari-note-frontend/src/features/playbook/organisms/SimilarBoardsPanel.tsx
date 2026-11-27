'use client';

import { Button } from '@/components/ui/button';
import type { PlaybookSimilarBoardDto } from '@/lib/api/types';
import { ResearchPanel } from './ResearchPanel';

interface SimilarBoardsPanelProps {
  plain?: boolean;
  similar: PlaybookSimilarBoardDto[];
  isBusy: boolean;
  onView: (boardId: string) => void;
  onMerge: (boardId: string) => void;
}

// 盤面の違いが少ないノードを出す。同じ形かどうかはユーザーが決める
export function SimilarBoardsPanel({ plain, similar, isBusy, onView, onMerge }: SimilarBoardsPanelProps) {
  if (similar.length === 0) return null;

  return (
    <ResearchPanel plain={plain} title="似ている局面" note="まとめると研究メモとグラフのノードを共有します">
      <ul className="flex flex-col gap-2">
        {similar.map((s) => (
          <li key={s.boardId} className="flex flex-col gap-2 rounded-lg border border-brand-border/60 p-3">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-sm font-bold text-brand-text">{s.nodeName || '名前のない局面'}</span>
              <div className="ml-auto flex gap-1">
                <Button size="sm" variant="ghost" onClick={() => onView(s.boardId)}>
                  盤で見る
                </Button>
                <Button size="sm" disabled={isBusy} className="bg-brand-primary-dark text-white hover:bg-brand-text" onClick={() => onMerge(s.boardId)}>
                  まとめる
                </Button>
              </div>
            </div>
            <p className="text-sm text-brand-text">違い: {s.diff || 'なし'}</p>
          </li>
        ))}
      </ul>
    </ResearchPanel>
  );
}
