import { type PlaybookTree, type TreeMove, nodeIdOf } from './playbookTree';
import { destinationOf, moveLabel } from './shogiRules';

export interface ChainMove {
  usi: string;
  label: string;
  sfen: string;
}

export interface Chain {
  transitionId: number;
  from: number;
  to: number;
  moves: ChainMove[];
  condition: string;
  isConditional: boolean;
}

export interface CollapsedNode {
  id: number;
  label: string;
  positionCount: number;
  sfen: string;
  isStub: boolean;
}

export interface CollapsedGraph {
  nodes: CollapsedNode[];
  chains: Chain[];
  rootId: number;
}

const ROOT_LABEL = '開始';
const UNNAMED_LABEL = '名前なし';

interface Edge {
  fromSfen: string;
  first: TreeMove;
  moves: TreeMove[];
  endNodeId?: number;
}

// ノードから出る遷移を、次のノードか行き止まりまで辿って 1 本の辺にする。ノードでない局面は出る遷移が 1 本以下
function edgesOf(tree: PlaybookTree): Edge[] {
  const edges: Edge[] = [];
  for (const board of tree.boards.values()) {
    if (board.nodeId === undefined) continue;
    for (const first of tree.children.get(board.sfen) ?? []) {
      const moves = [first];
      let cursor = first.child;
      const visited = new Set([board.sfen]);
      while (nodeIdOf(tree, cursor) === undefined && !visited.has(cursor)) {
        visited.add(cursor);
        const next = tree.children.get(cursor)?.[0];
        if (!next) break;
        moves.push(next);
        cursor = next.child;
      }
      edges.push({ fromSfen: board.sfen, first, moves, endNodeId: nodeIdOf(tree, cursor) });
    }
  }
  return edges;
}

// ノードを点、ノードからノードへの遷移の並びを辺として描く。辺の手は「出る手 ⋮ 入る手」に畳む
export function collapseGraph(tree: PlaybookTree): CollapsedGraph {
  const rootId = nodeIdOf(tree, tree.rootSfen)!;
  const boardsOf = (nodeId: number) => [...tree.boards.values()].filter((b) => b.nodeId === nodeId);

  const nodes: CollapsedNode[] = [...tree.nodes.values()].map((n) => {
    const boards = boardsOf(n.id);
    return {
      id: n.id,
      label: n.name || (n.id === rootId ? ROOT_LABEL : UNNAMED_LABEL),
      positionCount: boards.length,
      sfen: n.id === rootId ? tree.rootSfen : boards[0].sfen,
      isStub: false,
    };
  });

  let stubId = 0;
  const stub = (sfen: string) => {
    stubId -= 1;
    nodes.push({ id: stubId, label: '', positionCount: 1, sfen, isStub: true });
    return stubId;
  };

  const edges = edgesOf(tree);
  const conditional = conditionalTransitions(tree, edges);
  const chains: Chain[] = edges.map((e) => {
    const moves: ChainMove[] = [];
    let sfen = e.fromSfen;
    let previousTo: number | undefined;
    for (const m of e.moves) {
      moves.push({ usi: m.usi, label: moveLabel(sfen, m.usi, previousTo), sfen: m.child });
      previousTo = destinationOf(m.usi);
      sfen = m.child;
    }
    return {
      transitionId: e.first.transition.id,
      from: nodeIdOf(tree, e.fromSfen)!,
      to: e.endNodeId ?? stub(sfen),
      moves,
      condition: e.first.transition.condition,
      isConditional: conditional.has(e.first.transition.id),
    };
  });

  return { nodes, chains, rootId };
}

// まとめた局面から同じ手を指して行き先が分かれるのは、局面の違いで手の意味が変わるところ
function conditionalTransitions(tree: PlaybookTree, edges: Edge[]): Set<number> {
  const groups = new Map<string, Edge[]>();
  for (const e of edges) {
    const key = `${nodeIdOf(tree, e.fromSfen)}:${e.first.usi}`;
    groups.set(key, [...(groups.get(key) ?? []), e]);
  }
  const result = new Set<number>();
  for (const group of groups.values()) {
    const destinations = new Set(group.map((e) => (e.endNodeId !== undefined ? `node:${e.endNodeId}` : `open:${e.first.transition.id}`)));
    if (destinations.size > 1) group.forEach((e) => result.add(e.first.transition.id));
  }
  return result;
}
