import type { PlaybookGraphDto, PlaybookGraphNodeDto } from '@/lib/api/types';

export interface ChainMove {
  label: string;
  groupId: number;
  positionId: string;
}

export interface Chain {
  from: number;
  to: number;
  moves: ChainMove[];
  conditions: string[];
  isConditional: boolean;
}

export interface CollapsedNode {
  id: number;
  label: string;
  positionCount: number;
  positionId: string;
  isStub: boolean;
}

export interface CollapsedGraph {
  nodes: CollapsedNode[];
  chains: Chain[];
  rootId: number;
}

const ROOT_LABEL = '開始';

// 名前を付けた形だけをノードにし、その間の名前のない局面は手順として辺にまとめる
export function collapseGraph(graph: PlaybookGraphDto): CollapsedGraph {
  const byId = new Map<number, PlaybookGraphNodeDto>(graph.nodes.map((n) => [n.groupId, n]));
  const isNamed = (id: number) => id === graph.rootGroupId || (byId.get(id)?.name ?? '') !== '';
  const outgoing = new Map<number, typeof graph.edges>();
  for (const e of graph.edges) {
    const list = outgoing.get(e.fromGroupId) ?? [];
    list.push(e);
    outgoing.set(e.fromGroupId, list);
  }

  const nodes: CollapsedNode[] = graph.nodes
    .filter((n) => isNamed(n.groupId))
    .map((n) => ({
      id: n.groupId,
      label: n.name || ROOT_LABEL,
      positionCount: n.positionCount,
      positionId: n.representativePositionId,
      isStub: false,
    }));

  const chains: Chain[] = [];
  let stubId = 0;
  const walk = (from: number, at: number, moves: ChainMove[], conditions: string[], conditional: boolean, visited: Set<number>) => {
    for (const e of outgoing.get(at) ?? []) {
      const target = byId.get(e.toGroupId);
      if (!target) continue;
      const nextMoves = [...moves, { label: e.label, groupId: e.toGroupId, positionId: target.representativePositionId }];
      const nextConditions = e.condition ? [...conditions, e.condition] : conditions;
      const nextConditional = conditional || e.isConditional;
      const ends = isNamed(e.toGroupId) || visited.has(e.toGroupId) || (outgoing.get(e.toGroupId) ?? []).length === 0;
      if (isNamed(e.toGroupId)) {
        chains.push({ from, to: e.toGroupId, moves: nextMoves, conditions: nextConditions, isConditional: nextConditional });
      } else if (ends) {
        stubId -= 1;
        nodes.push({ id: stubId, label: '', positionCount: 1, positionId: target.representativePositionId, isStub: true });
        chains.push({ from, to: stubId, moves: nextMoves, conditions: nextConditions, isConditional: nextConditional });
      } else {
        walk(from, e.toGroupId, nextMoves, nextConditions, nextConditional, new Set([...visited, e.toGroupId]));
      }
    }
  };
  for (const n of graph.nodes) {
    if (isNamed(n.groupId)) walk(n.groupId, n.groupId, [], [], false, new Set([n.groupId]));
  }

  return { nodes, chains, rootId: graph.rootGroupId };
}
