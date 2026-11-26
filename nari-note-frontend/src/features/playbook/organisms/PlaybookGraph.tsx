'use client';

import { useEffect, useMemo, useRef } from 'react';
import { GitBranch } from 'lucide-react';
import type { PlaybookGraphDto } from '@/lib/api/types';
import { cn } from '@/lib/utils';
import { type Chain, type ChainMove, type CollapsedNode, collapseGraph } from '../utils/collapseGraph';
import { NODE_HEIGHT, layoutGraph } from '../utils/graphLayout';

interface PlaybookGraphProps {
  graph: PlaybookGraphDto;
  className?: string;
  onSelect: (positionId: string) => void;
}

const MOVE_CHAR_WIDTH = 12;
// 形を出る手は形のすぐ下、形に入る手は形のすぐ上に置く
function labelWidth(chain: Chain): number {
  const moveWidth = Math.max(...chain.moves.map((m) => m.label.length * MOVE_CHAR_WIDTH + 16));
  const conditionWidth = chain.conditions.reduce((max, c) => Math.max(max, c.length * 11 + 34), 0);
  return Math.max(moveWidth, conditionWidth) + 8;
}

function MoveChip({ move, isCurrent, onSelect }: { move: ChainMove; isCurrent: boolean; onSelect: (positionId: string) => void }) {
  return (
    <button
      type="button"
      aria-current={isCurrent ? 'location' : undefined}
      className={cn(
        'whitespace-nowrap rounded-full border px-2 text-xs font-bold leading-5',
        isCurrent ? 'border-brand-primary-dark bg-brand-primary-dark text-white' : 'border-brand-border/60 bg-white text-brand-text hover:bg-brand-primary-light',
      )}
      onClick={() => onSelect(move.positionId)}
    >
      {move.label}
    </button>
  );
}

export function PlaybookGraph({ graph, className, onSelect }: PlaybookGraphProps) {
  const layout = useMemo(() => {
    const collapsed = collapseGraph(graph);
    const chains = collapsed.chains.map((c) => ({ ...c, labelWidth: labelWidth(c) }));
    return layoutGraph<CollapsedNode, (typeof chains)[number]>(collapsed.nodes, chains, collapsed.rootId);
  }, [graph]);
  const containerRef = useRef<HTMLDivElement>(null);
  const current = graph.currentGroupId;

  useEffect(() => {
    const box = containerRef.current;
    if (!box) return;
    const node = layout.nodes.find((p) => p.node.id === current);
    const edge = layout.edges.find((e) => e.edge.moves.some((m) => m.groupId === current));
    const target = node ? { x: node.x, y: node.y } : edge ? { x: edge.lane.x, y: (edge.lane.top + edge.lane.bottom) / 2 } : null;
    if (!target) return;
    box.scrollTo({
      left: Math.max(0, target.x - box.clientWidth / 2),
      top: Math.max(0, target.y - box.clientHeight / 2),
      behavior: 'smooth',
    });
  }, [current, layout]);

  return (
    <div ref={containerRef} className={cn('overflow-auto rounded-lg border border-brand-border/60 bg-brand-bg-light/50', className)}>
      <div className="relative" style={{ width: layout.width, height: layout.height }}>
        <svg width={layout.width} height={layout.height} className="absolute inset-0" aria-hidden="true">
          {layout.edges.map(({ edge, path, lane }, i) => (
            <path
              key={i}
              // 名前を付けた形にたどり着かない手順は、省略記号のところで線を止める
              d={edge.to < 0 ? `${path.split(' L ')[0]} L ${lane.x} ${(lane.top + lane.bottom) / 2}` : path}
              fill="none"
              strokeWidth={edge.isConditional ? 2 : 1.5}
              strokeDasharray={edge.isConditional ? '6 5' : undefined}
              className={edge.isConditional ? 'stroke-orange-700' : 'stroke-shogi-koma-edge'}
            />
          ))}
        </svg>

        {layout.edges.map(({ edge, lane }, i) => {
          const open = edge.to < 0;
          const first = edge.moves[0];
          const last = edge.moves[edge.moves.length - 1];
          const hidden = open ? edge.moves.slice(1) : edge.moves.slice(1, -1);
          const hiddenCurrent = hidden.find((m) => m.groupId === current);
          const single = edge.moves.length === 1 && !open;
          const middle = (lane.top + lane.bottom) / 2;
          // 名前の付いた行き先は札の方を強調するので、手順の中では強調しない
          const isCurrentMove = (m: ChainMove) => m.groupId === current && m.groupId !== edge.to;
          return (
            <div key={`labels-${i}`}>
              <div
                className={cn('absolute flex -translate-x-1/2 flex-col items-center gap-0.5', single && '-translate-y-1/2')}
                style={{ left: lane.x, top: single ? middle : lane.top }}
              >
                {edge.conditions.map((c) => (
                  <span
                    key={c}
                    className="flex items-center gap-1 whitespace-nowrap rounded-full border border-orange-200 bg-orange-50 px-1.5 text-[11px] leading-4 text-orange-800"
                  >
                    <GitBranch className="h-3 w-3" aria-hidden="true" />
                    {c}
                  </span>
                ))}
                <MoveChip move={first} isCurrent={isCurrentMove(first)} onSelect={onSelect} />
              </div>
              {hidden.length > 0 && (
                <span
                  title={hiddenCurrent ? `${hidden.length}手省略（今は${hiddenCurrent.label}）` : `${hidden.length}手省略`}
                  className={cn(
                    'absolute -translate-x-1/2 -translate-y-1/2 rounded-full px-1 text-sm font-bold leading-4',
                    hiddenCurrent ? 'bg-brand-primary-dark text-white' : 'bg-brand-bg-light text-brand-secondary-text',
                  )}
                  style={{ left: lane.x, top: open ? middle + 8 : middle + 6 }}
                >
                  ⋮
                </span>
              )}
              {!single && !open && (
                <div className="absolute -translate-x-1/2 -translate-y-full" style={{ left: lane.x, top: lane.bottom }}>
                  <MoveChip move={last} isCurrent={isCurrentMove(last)} onSelect={onSelect} />
                </div>
              )}
            </div>
          );
        })}

        {layout.nodes.filter(({ node }) => !node.isStub).map(({ node, x, y, width }) => {
          const isCurrent = node.id === current;
          return (
            <button
              key={node.id}
              type="button"
              aria-current={isCurrent ? 'location' : undefined}
              className={cn(
                'absolute flex -translate-x-1/2 -translate-y-1/2 items-center justify-center gap-1.5 whitespace-nowrap rounded-lg px-2 text-[13px] font-bold text-brand-text outline-none focus-visible:ring-2 focus-visible:ring-brand-primary',
                isCurrent ? 'border-2 border-brand-primary-dark bg-brand-primary-light' : 'border border-brand-border bg-white hover:border-brand-primary-dark',
              )}
              style={{ left: x, top: y, width, height: NODE_HEIGHT }}
              onClick={() => onSelect(node.positionId)}
            >
              {node.label}
              {node.positionCount > 1 && <span className="text-[10px] text-brand-primary-dark">×{node.positionCount}</span>}
            </button>
          );
        })}
      </div>
    </div>
  );
}
