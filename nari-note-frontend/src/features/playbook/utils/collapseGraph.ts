import type { PlaybookTree, TreeMove } from './playbookTree';
import { destinationOf, moveLabel } from './shogiRules';

export interface ChainMove {
  usi: string;
  label: string;
  sfen: string;
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
  sfen: string;
  isStub: boolean;
}

export interface CollapsedGraph {
  nodes: CollapsedNode[];
  chains: Chain[];
  rootId: number;
}

const ROOT_LABEL = '開始';
// ノードの ID は 1 から、省略記号の行き先は負の数で振るので、ノードに属さない開始局面は 0 にする
const ROOT_ID = 0;

function rootIdOf(tree: PlaybookTree): number {
  return tree.boards.get(tree.rootSfen)?.nodeId ?? ROOT_ID;
}

// グラフに描く点の ID。開始局面と名前を付けたノードの局面だけが点になる
export function graphNodeIdOf(tree: PlaybookTree, sfen: string): number | undefined {
  const rootId = rootIdOf(tree);
  if (sfen === tree.rootSfen) return rootId;
  const nodeId = tree.boards.get(sfen)?.nodeId;
  if (nodeId == null) return undefined;
  return nodeId === rootId || (tree.nodes.get(nodeId)?.name ?? '') !== '' ? nodeId : undefined;
}

// 開始局面と名前を付けたノードだけを描き、その間の局面は手の並びとして辺にまとめる
export function collapseGraph(tree: PlaybookTree): CollapsedGraph {
  const rootId = rootIdOf(tree);
  const sfensByNode = new Map<number, string[]>();
  for (const board of tree.boards.values()) {
    const id = graphNodeIdOf(tree, board.sfen);
    if (id !== undefined) sfensByNode.set(id, [...(sfensByNode.get(id) ?? []), board.sfen]);
  }

  const nodes: CollapsedNode[] = [...sfensByNode.entries()].map(([id, sfens]) => ({
    id,
    label: tree.nodes.get(id)?.name || ROOT_LABEL,
    positionCount: sfens.length,
    sfen: id === rootId && !tree.nodes.has(id) ? tree.rootSfen : sfens[0],
    isStub: false,
  }));

  const chains: Chain[] = [];
  let stubId = 0;
  const stub = (sfen: string) => {
    stubId -= 1;
    nodes.push({ id: stubId, label: '', positionCount: 1, sfen, isStub: true });
    return stubId;
  };

  const conditional = conditionalTransitions(tree);
  const walk = (
    from: number,
    parentSfen: string,
    move: TreeMove,
    moves: ChainMove[],
    conditions: string[],
    isConditional: boolean,
    visited: Set<string>,
  ) => {
    const previousTo = moves.length > 0 ? destinationOf(moves[moves.length - 1].usi) : undefined;
    const nextMoves = [...moves, { usi: move.usi, label: moveLabel(parentSfen, move.usi, previousTo), sfen: move.child }];
    const nextConditions = move.transition.condition ? [...conditions, move.transition.condition] : conditions;
    const target = graphNodeIdOf(tree, move.child);
    const outgoing = tree.children.get(move.child) ?? [];

    if (target !== undefined) {
      chains.push({ from, to: target, moves: nextMoves, conditions: nextConditions, isConditional });
    } else if (visited.has(move.child) || outgoing.length === 0) {
      chains.push({ from, to: stub(move.child), moves: nextMoves, conditions: nextConditions, isConditional });
    } else {
      const nextVisited = new Set([...visited, move.child]);
      for (const next of outgoing) walk(from, move.child, next, nextMoves, nextConditions, isConditional, nextVisited);
    }
  };

  for (const [id, sfens] of sfensByNode) {
    for (const sfen of sfens) {
      for (const move of tree.children.get(sfen) ?? []) {
        walk(id, sfen, move, [], [], conditional.has(move.transition.id), new Set([sfen]));
      }
    }
  }

  return { nodes, chains, rootId };
}

// まとめた局面から同じ手を指して行き先が分かれるのは、局面の違いで手の意味が変わるところ
function conditionalTransitions(tree: PlaybookTree): Set<number> {
  const groups = new Map<string, TreeMove[]>();
  for (const board of tree.boards.values()) {
    if (board.nodeId == null) continue;
    for (const move of tree.children.get(board.sfen) ?? []) {
      const key = `${board.nodeId}:${move.usi}`;
      groups.set(key, [...(groups.get(key) ?? []), move]);
    }
  }
  const destinationOfMove = (move: TreeMove) => {
    const nodeId = tree.boards.get(move.child)?.nodeId;
    return nodeId != null ? `node:${nodeId}` : `board:${move.transition.toBoardId}`;
  };
  const result = new Set<number>();
  for (const moves of groups.values()) {
    if (new Set(moves.map(destinationOfMove)).size > 1) moves.forEach((m) => result.add(m.transition.id));
  }
  return result;
}
