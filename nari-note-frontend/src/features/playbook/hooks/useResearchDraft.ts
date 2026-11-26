'use client';

import { useCallback, useMemo, useState } from 'react';

export type DraftParent = { kind: 'saved'; positionId: string } | { kind: 'draft'; sfen: string };

export interface DraftNode {
  sfen: string;
  parent: DraftParent;
  usi: string;
  label: string;
}

export interface DraftLine {
  fromPositionId: string;
  usis: string[];
}

const sameParent = (a: DraftParent, b: DraftParent) =>
  a.kind === b.kind && (a.kind === 'saved' ? a.positionId === (b as typeof a).positionId : a.sfen === (b as typeof a).sfen);

// 研究中に指した手は保存するまで手元だけに置く。局面は SFEN で見分ける
export function useResearchDraft() {
  const [nodes, setNodes] = useState<Map<string, DraftNode>>(() => new Map());

  const add = useCallback((node: DraftNode) => {
    setNodes((prev) => {
      if (prev.has(node.sfen)) return prev;
      const next = new Map(prev);
      next.set(node.sfen, node);
      return next;
    });
  }, []);

  const childrenOf = useCallback((parent: DraftParent) => [...nodes.values()].filter((n) => sameParent(n.parent, parent)), [nodes]);

  // 局面から、手元の手順をさかのぼって最初の保存済み局面までの手を並べる
  const lineTo = useCallback(
    (sfen: string): { anchorId: string; nodes: DraftNode[] } | null => {
      const chain: DraftNode[] = [];
      let node = nodes.get(sfen);
      while (node) {
        chain.unshift(node);
        if (node.parent.kind === 'saved') return { anchorId: node.parent.positionId, nodes: chain };
        node = nodes.get(node.parent.sfen);
      }
      return null;
    },
    [nodes],
  );

  const remove = useCallback((sfen: string) => {
    setNodes((prev) => {
      const doomed = new Set([sfen]);
      let grew = true;
      while (grew) {
        grew = false;
        for (const n of prev.values()) {
          if (n.parent.kind === 'draft' && doomed.has(n.parent.sfen) && !doomed.has(n.sfen)) {
            doomed.add(n.sfen);
            grew = true;
          }
        }
      }
      return new Map([...prev].filter(([key]) => !doomed.has(key)));
    });
  }, []);

  const clear = useCallback(() => setNodes(new Map()), []);

  // 保存では、手元の木の葉ごとに保存済み局面からの手順として送る
  const lines = useMemo((): DraftLine[] => {
    const parents = new Set([...nodes.values()].filter((n) => n.parent.kind === 'draft').map((n) => (n.parent as { sfen: string }).sfen));
    return [...nodes.values()]
      .filter((n) => !parents.has(n.sfen))
      .map((leaf) => lineTo(leaf.sfen))
      .filter((l): l is { anchorId: string; nodes: DraftNode[] } => l !== null)
      .map((l) => ({ fromPositionId: l.anchorId, usis: l.nodes.map((n) => n.usi) }));
  }, [nodes, lineTo]);

  return { nodes, count: nodes.size, add, childrenOf, lineTo, remove, clear, lines };
}
