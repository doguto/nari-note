'use client';

import { ArrowRight, GitBranch, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { formatScore } from '../utils/score';
import { ResearchPanel } from './ResearchPanel';
import { SuggestionChips } from './SuggestionChips';

export interface ResearchMove {
  usi: string;
  label: string;
  isDraft: boolean;
  // 保存済みの手は、ノードから出る辺の 1 手目。条件はその辺に書く
  transitionId?: number;
  condition: string;
  conditionSuggestions: string[];
}

interface NextMovesPanelProps {
  moves: ResearchMove[];
  // ノードでない局面では、保存済みの手を出さない
  isNode: boolean;
  scores: Record<string, number>;
  selectedIndex: number;
  className?: string;
  onSelect: (index: number) => void;
  onGo: (usi: string) => void;
  onChangeCondition: (transitionId: number, condition: string) => void;
  onDeleteTransition: (transitionId: number) => void;
}

const fieldClass =
  'h-9 min-w-0 flex-1 rounded-md border border-brand-border bg-white px-2.5 text-sm text-brand-text focus:outline-none focus:ring-2 focus:ring-brand-primary-light';

export function NextMovesPanel({
  moves,
  isNode,
  scores,
  selectedIndex,
  className,
  onSelect,
  onGo,
  onChangeCondition,
  onDeleteTransition,
}: NextMovesPanelProps) {
  return (
    <ResearchPanel title="ここからの手" icon={<ArrowRight className="h-4 w-4" aria-hidden="true" />} note={`${moves.length}手`} className={className}>
      {moves.length === 0 ? (
        <p className="text-sm leading-relaxed text-brand-secondary-text">
          {isNode
            ? 'まだ手がありません。盤で指すか、AI候補手を押すと進めます。'
            : 'ノードでない局面では、保存済みの手は出しません。→ キーで手順を進められます。'}
        </p>
      ) : (
        <ul className="-mx-2 flex min-h-0 flex-1 flex-col overflow-y-auto px-2">
          {moves.map((m, i) => {
            const selected = i === selectedIndex;
            const score = scores[m.usi];
            const transitionId = m.transitionId;
            return (
              <li
                key={m.usi}
                className={cn('border-t border-brand-border/40 first:border-t-0', selected && 'my-1 rounded-lg border-t-0 bg-orange-50/60 px-2 pb-3')}
              >
                <div className="grid min-h-11 grid-cols-[6.5rem_minmax(0,1fr)_3rem] items-center gap-2">
                  <button
                    type="button"
                    title="この手を指す"
                    className="justify-self-start rounded-md px-1 py-1 text-left text-[15px] font-bold text-brand-text hover:bg-brand-primary-light"
                    onClick={() => onGo(m.usi)}
                  >
                    {m.label}
                  </button>
                  {m.isDraft ? (
                    <span className="self-center justify-self-start rounded-full border border-dashed border-brand-border px-2 text-[11px] leading-5 text-brand-secondary-text">
                      保存していない手
                    </span>
                  ) : (
                    <button
                      type="button"
                      aria-expanded={selected}
                      className="flex min-w-0 items-center gap-1 self-stretch text-left text-xs text-orange-800"
                      onClick={() => onSelect(selected ? -1 : i)}
                    >
                      {m.condition && (
                        <>
                          <GitBranch className="h-3.5 w-3.5 flex-none" aria-hidden="true" />
                          <span className="truncate">{m.condition}</span>
                        </>
                      )}
                    </button>
                  )}
                  <span className="text-right text-sm font-bold tabular-nums text-brand-text">{score !== undefined ? formatScore(score) : ''}</span>
                </div>
                {selected && !m.isDraft && (
                  <div className="flex flex-col gap-2">
                    {transitionId !== undefined && (
                      <>
                        <label className="flex items-center gap-2">
                          <span className="flex w-12 flex-none items-center gap-1 text-xs font-bold text-orange-800">
                            <GitBranch className="h-3.5 w-3.5" aria-hidden="true" />
                            もし
                          </span>
                          <input
                            key={m.condition}
                            type="text"
                            defaultValue={m.condition}
                            placeholder="条件なし"
                            maxLength={100}
                            className={fieldClass}
                            onBlur={(e) => {
                              if (e.target.value !== m.condition) onChangeCondition(transitionId, e.target.value);
                            }}
                          />
                        </label>
                        <div className="pl-14">
                          <SuggestionChips label="候補" suggestions={m.conditionSuggestions} onPick={(c) => onChangeCondition(transitionId, c)} />
                        </div>
                      </>
                    )}
                    {transitionId !== undefined && (
                      <div className="flex justify-end">
                        <Button size="sm" variant="ghost" className="text-red-700" onClick={() => onDeleteTransition(transitionId)}>
                          <Trash2 aria-hidden="true" />
                          この辺を削除
                        </Button>
                      </div>
                    )}
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </ResearchPanel>
  );
}
