'use client';

import { Check, Plus } from 'lucide-react';
import type { EngineLineDto } from '@/lib/api/types';
import { ANALYSIS_ENGINES, type AnalysisEngineId } from '../utils/analysisEngine';
import { formatScore } from '../utils/score';

interface EngineLinesPanelProps {
  isLoading: boolean;
  isAvailable: boolean;
  hasError: boolean;
  lines: EngineLineDto[];
  engine: AnalysisEngineId;
  onEngineChange: (engine: AnalysisEngineId) => void;
  registeredUsis: string[];
  disabled?: boolean;
  onPlay: (usi: string) => void;
}

export function EngineLinesPanel({ engine, onEngineChange, ...rest }: EngineLinesPanelProps) {
  const unavailable = !rest.isLoading && !rest.hasError && !rest.isAvailable;
  return (
    <div className="flex flex-col gap-3">
      {!unavailable && (
        <div role="radiogroup" aria-label="解析に使うAI" className="grid grid-cols-2 gap-1 rounded-lg bg-brand-bg-gradient-to p-1">
          {ANALYSIS_ENGINES.map((e) => (
            <button
              key={e.id}
              type="button"
              role="radio"
              aria-checked={engine === e.id}
              className="flex min-h-10 flex-col items-center justify-center rounded-md px-2 text-xs text-brand-secondary-text hover:text-brand-text aria-checked:bg-white aria-checked:font-bold aria-checked:text-brand-primary-dark aria-checked:shadow-sm"
              onClick={() => onEngineChange(e.id)}
            >
              <span>{e.label}</span>
              <span className="text-[11px] font-normal">{e.description}</span>
            </button>
          ))}
        </div>
      )}
      <EngineLines {...rest} />
    </div>
  );
}

function EngineLines({
  isLoading,
  isAvailable,
  hasError,
  lines,
  registeredUsis,
  disabled,
  onPlay,
}: Omit<EngineLinesPanelProps, 'engine' | 'onEngineChange'>) {
  if (hasError) return <p className="text-sm text-red-600">解析に失敗しました。時間をおいて開き直してください。</p>;
  if (isLoading) return <p className="text-sm text-brand-secondary-text">解析中...</p>;
  if (!isAvailable) return <p className="text-sm text-brand-secondary-text">この環境ではAIを使えません。</p>;
  if (lines.length === 0) return <p className="text-sm text-brand-secondary-text">終局しています。</p>;

  return (
    <ul aria-label="AI候補手" className="flex flex-col">
      {lines.map((l, i) => {
        const registered = registeredUsis.includes(l.usi);
        return (
          <li key={l.usi} className="border-t border-brand-border/40 first:border-t-0">
            <button
              type="button"
              disabled={disabled}
              title={registered ? 'この手へ進む' : 'この手を指して記録する'}
              className="grid min-h-12 w-full grid-cols-[1.25rem_minmax(0,1fr)_3.5rem_2rem] items-center gap-2 rounded-md px-1 text-left hover:bg-brand-primary-light disabled:opacity-50"
              onClick={() => onPlay(l.usi)}
            >
              <span className="text-xs tabular-nums text-brand-secondary-text">{i + 1}</span>
              <span className="flex min-w-0 flex-col">
                <span className="text-[15px] font-bold text-brand-text">{l.label}</span>
                <span className="truncate text-xs text-brand-secondary-text">{l.pv.slice(1).join(' ')}</span>
              </span>
              <span className="text-right text-sm font-bold tabular-nums text-brand-text">{formatScore(l.score)}</span>
              <span className="flex justify-center">
                {registered ? (
                  <Check className="h-4 w-4 text-brand-primary-dark" aria-label="登録済み" />
                ) : (
                  <span className="flex h-7 w-7 items-center justify-center rounded-md border border-brand-border text-brand-text">
                    <Plus className="h-4 w-4" aria-hidden="true" />
                  </span>
                )}
              </span>
            </button>
          </li>
        );
      })}
    </ul>
  );
}
