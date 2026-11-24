'use client';

import { Button } from '@/components/ui/button';
import type { ShapeProposalDto } from '@/lib/api/types';
import { ResearchPanel } from './ResearchPanel';

interface ShapeProposalsPanelProps {
  plain?: boolean;
  proposals: ShapeProposalDto[];
  isBusy: boolean;
  onView: (positionId: number) => void;
  onMerge: (candidateId: number) => void;
  onReject: (candidateId: number) => void;
}

const VERDICT_LABELS: Record<string, string> = {
  same: '同じ',
  same_with_delta: 'Δ付きで同じ',
  same_until: '浅い深さでは同じ',
};

export function ShapeProposalsPanel({ plain, proposals, isBusy, onView, onMerge, onReject }: ShapeProposalsPanelProps) {
  if (proposals.length === 0) return null;

  return (
    <ResearchPanel plain={plain} title="同じ形の候補" note="まとめると研究メモとグラフのノードを共有します">
      <ul className="flex flex-col gap-2">
        {proposals.map((p) => (
          <li key={p.candidateId} className="flex flex-col gap-2 rounded-lg border border-brand-border/60 p-3">
            <div className="flex flex-wrap items-center gap-2">
              <span
                className={
                  p.verdict === 'same'
                    ? 'rounded-md bg-brand-primary-light px-2 py-0.5 text-xs font-bold text-brand-text'
                    : 'rounded-md bg-orange-100 px-2 py-0.5 text-xs font-bold text-orange-900'
                }
              >
                {VERDICT_LABELS[p.verdict] ?? p.verdict}
              </span>
              {p.delta != null && (
                <span className="text-xs text-brand-secondary-text">Δ {p.delta > 0 ? `+${p.delta}` : p.delta}</span>
              )}
              {p.verdict === 'same_until' && p.confirmedDepth != null && (
                <span className="text-xs text-brand-secondary-text">{p.confirmedDepth}手先で分かれる</span>
              )}
              <div className="ml-auto flex gap-1">
                <Button size="sm" variant="ghost" onClick={() => onView(p.otherPositionId)}>
                  盤で見る
                </Button>
                <Button size="sm" variant="outline" disabled={isBusy} onClick={() => onReject(p.candidateId)}>
                  別のまま
                </Button>
                <Button
                  size="sm"
                  disabled={isBusy}
                  className="bg-brand-primary-dark text-white hover:bg-brand-text"
                  onClick={() => onMerge(p.candidateId)}
                >
                  まとめる
                </Button>
              </div>
            </div>
            <p className="text-sm text-brand-text">違い: {p.diff || 'なし'}</p>
            {p.otherName && <p className="text-xs text-brand-secondary-text">相手の形: {p.otherName}</p>}
          </li>
        ))}
      </ul>
    </ResearchPanel>
  );
}
