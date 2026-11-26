'use client';

import { useCallback, useMemo, useState } from 'react';

export interface DraftNode {
  sfen: string;
  parentSfen: string;
  usi: string;
  label: string;
}

export interface DraftLine {
  // 手元の手順がつながる、保存済みの局面
  anchorSfen: string;
  nodes: DraftNode[];
}

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

  const childrenOf = useCallback((sfen: string) => [...nodes.values()].filter((n) => n.parentSfen === sfen), [nodes]);

  // 局面から手元の手順をさかのぼり、最初の保存済み局面までの手を並べる
  const lineTo = useCallback(
    (sfen: string): DraftLine | null => {
      const chain: DraftNode[] = [];
      let node = nodes.get(sfen);
      while (node) {
        chain.unshift(node);
        const parent = nodes.get(node.parentSfen);
        if (!parent) return { anchorSfen: node.parentSfen, nodes: chain };
        node = parent;
      }
      return null;
    },
    [nodes],
  );

  const clear = useCallback(() => setNodes(new Map()), []);

  // 保存では、手元の木の葉ごとに、保存済みの局面からの手順として送る
  const lines = useMemo((): DraftLine[] => {
    const parents = new Set([...nodes.values()].map((n) => n.parentSfen));
    return [...nodes.values()]
      .filter((n) => !parents.has(n.sfen))
      .map((leaf) => lineTo(leaf.sfen))
      .filter((l): l is DraftLine => l !== null);
  }, [nodes, lineTo]);

  const get = useCallback((sfen: string) => nodes.get(sfen), [nodes]);

  return { count: nodes.size, add, get, childrenOf, lineTo, clear, lines };
}
