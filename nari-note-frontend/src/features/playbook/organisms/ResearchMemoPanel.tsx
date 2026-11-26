'use client';

import { ResearchPanel } from './ResearchPanel';

interface ResearchMemoPanelProps {
  plain?: boolean;
  memo: string;
  onSave: (memo: string) => void;
}

// 研究メモは形ごとに 1 つで、まとめた局面すべてで共有する
export function ResearchMemoPanel({ plain, memo, onSave }: ResearchMemoPanelProps) {
  return (
    <ResearchPanel plain={plain} title="研究メモ" note="この形にまとめた局面で共有します。書くとノードになります">
      <textarea
        defaultValue={memo}
        rows={5}
        maxLength={2000}
        aria-label="研究メモ"
        placeholder="この形の狙い、結論、気になる変化など"
        className="resize-y rounded-lg border border-brand-border px-3 py-2 text-sm leading-relaxed text-brand-text focus:outline-none focus:ring-2 focus:ring-brand-primary-light"
        onBlur={(e) => {
          if (e.target.value !== memo) onSave(e.target.value);
        }}
      />
    </ResearchPanel>
  );
}
