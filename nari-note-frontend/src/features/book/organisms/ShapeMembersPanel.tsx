'use client';

import { Button } from '@/components/ui/button';
import type { ShapeMemberDto } from '@/lib/api/types';
import { ResearchPanel } from './ResearchPanel';

interface ShapeMembersPanelProps {
  plain?: boolean;
  members: ShapeMemberDto[];
  isBusy: boolean;
  onView: (positionId: number) => void;
  onSplit: (positionId: number) => void;
}

export function ShapeMembersPanel({ plain, members, isBusy, onView, onSplit }: ShapeMembersPanelProps) {
  if (members.length <= 1) return null;

  return (
    <ResearchPanel plain={plain} title="この形にまとめた局面" note={`${members.length}局面`}>
      <ul className="flex flex-col gap-2">
        {members.map((m) => (
          <li
            key={m.positionId}
            className={`flex flex-wrap items-center gap-2 rounded-lg border p-3 ${m.isCurrent ? 'border-brand-primary-dark' : 'border-brand-border/60'}`}
          >
            <span className="rounded-md bg-brand-primary-light px-2 py-0.5 text-xs font-bold text-brand-text">
              {m.isCurrent ? '表示中' : m.isRepresentative ? '代表局面' : 'メンバー'}
            </span>
            <span className="text-sm text-brand-text">{m.isCurrent ? 'この局面' : `違い: ${m.diff}`}</span>
            <div className="ml-auto flex gap-1">
              {!m.isCurrent && (
                <Button size="sm" variant="ghost" onClick={() => onView(m.positionId)}>
                  盤で見る
                </Button>
              )}
              <Button size="sm" variant="ghost" disabled={isBusy} onClick={() => onSplit(m.positionId)}>
                分ける
              </Button>
            </div>
          </li>
        ))}
      </ul>
    </ResearchPanel>
  );
}
