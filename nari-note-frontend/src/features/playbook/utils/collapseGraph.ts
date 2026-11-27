import type { PlaybookBoardTransitionDto } from '@/lib/api/types';
import type { PlaybookTree } from './playbookTree';
import { destinationOf, moveLabel, play } from './shogiRules';

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

// ノードを点、遷移を辺として描く。辺の手は「出る手 ⋮ 入る手」に畳むため、手順を指し直して記譜を作る
export function collapseGraph(tree: PlaybookTree): CollapsedGraph {
  const rootId = tree.boards.get(tree.rootSfen)!.nodeId;
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

  const conditional = conditionalTransitions(tree);
  const chains: Chain[] = tree.transitions.flatMap((t) => {
    const from = tree.boardsById.get(t.fromBoardId);
    if (!from) return [];
    const moves: ChainMove[] = [];
    let sfen = from.sfen;
    let previousTo: number | undefined;
    for (const usi of t.usis) {
      const child = play(sfen, usi);
      moves.push({ usi, label: moveLabel(sfen, usi, previousTo), sfen: child });
      previousTo = destinationOf(usi);
      sfen = child;
    }
    const to = t.toBoardId ? tree.boardsById.get(t.toBoardId)?.nodeId : undefined;
    return [
      {
        transitionId: t.id,
        from: from.nodeId,
        to: to ?? stub(sfen),
        moves,
        condition: t.condition,
        isConditional: conditional.has(t.id),
      },
    ];
  });

  return { nodes, chains, rootId };
}

// まとめた局面から同じ手を指して行き先が分かれるのは、局面の違いで手の意味が変わるところ
function conditionalTransitions(tree: PlaybookTree): Set<number> {
  const groups = new Map<string, PlaybookBoardTransitionDto[]>();
  for (const t of tree.transitions) {
    const from = tree.boardsById.get(t.fromBoardId);
    if (!from) continue;
    const key = `${from.nodeId}:${t.usis[0]}`;
    groups.set(key, [...(groups.get(key) ?? []), t]);
  }
  const destinationOfTransition = (t: PlaybookBoardTransitionDto) =>
    t.toBoardId ? `node:${tree.boardsById.get(t.toBoardId)?.nodeId}` : `open:${t.id}`;
  const result = new Set<number>();
  for (const transitions of groups.values()) {
    if (new Set(transitions.map(destinationOfTransition)).size > 1) transitions.forEach((t) => result.add(t.id));
  }
  return result;
}
