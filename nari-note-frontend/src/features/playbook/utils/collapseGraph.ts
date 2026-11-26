import type { PlaybookNodeStepDto } from '@/lib/api/types';
import type { PlaybookTree } from './playbookTree';
import { destinationOf, moveLabel, play } from './shogiRules';

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

// 名前を付けたノードだけを描き、その間の名前のないノードと手順の途中の局面は、手の並びとして辺にまとめる
export function collapseGraph(tree: PlaybookTree): CollapsedGraph {
  const rootId = tree.boards.get(tree.rootSfen)!.nodeId;
  const isNamed = (nodeId: number) => nodeId === rootId || (tree.nodes.get(nodeId)?.name ?? '') !== '';
  const stepsFrom = new Map<string, PlaybookNodeStepDto[]>();
  for (const s of tree.steps) stepsFrom.set(s.fromBoardId, [...(stepsFrom.get(s.fromBoardId) ?? []), s]);
  const boardsOf = (nodeId: number) => [...tree.boards.values()].filter((b) => b.nodeId === nodeId);

  const nodes: CollapsedNode[] = [...tree.nodes.values()]
    .filter((n) => isNamed(n.id))
    .map((n) => {
      const boards = boardsOf(n.id);
      return { id: n.id, label: n.name || ROOT_LABEL, positionCount: boards.length, sfen: boards[0].sfen, isStub: false };
    });

  const chains: Chain[] = [];
  let stubId = 0;
  const stub = (sfen: string) => {
    stubId -= 1;
    nodes.push({ id: stubId, label: '', positionCount: 1, sfen, isStub: true });
    return stubId;
  };

  const conditional = conditionalSteps(tree, stepsFrom);
  const walk = (
    from: number,
    step: PlaybookNodeStepDto,
    moves: ChainMove[],
    conditions: string[],
    isConditional: boolean,
    visited: Set<string>,
  ) => {
    let sfen = tree.boardsById.get(step.fromBoardId)!.sfen;
    let previousTo = moves.length > 0 ? destinationOf(moves[moves.length - 1].usi) : undefined;
    const nextMoves = [...moves];
    for (const usi of step.usis) {
      const child = play(sfen, usi);
      nextMoves.push({ usi, label: moveLabel(sfen, usi, previousTo), sfen: child });
      previousTo = destinationOf(usi);
      sfen = child;
    }
    const nextConditions = step.condition ? [...conditions, step.condition] : conditions;
    const target = step.toBoardId ? tree.boardsById.get(step.toBoardId) : undefined;
    const outgoing = target ? (stepsFrom.get(target.id) ?? []) : [];

    if (target && isNamed(target.nodeId)) {
      chains.push({ from, to: target.nodeId, moves: nextMoves, conditions: nextConditions, isConditional });
    } else if (!target || visited.has(target.id) || outgoing.length === 0) {
      chains.push({ from, to: stub(sfen), moves: nextMoves, conditions: nextConditions, isConditional });
    } else {
      for (const next of outgoing) walk(from, next, nextMoves, nextConditions, isConditional, new Set([...visited, target.id]));
    }
  };

  for (const node of nodes.filter((n) => !n.isStub)) {
    for (const board of boardsOf(node.id)) {
      for (const step of stepsFrom.get(board.id) ?? []) walk(node.id, step, [], [], conditional.has(step.id), new Set([board.id]));
    }
  }

  return { nodes, chains, rootId };
}

// まとめた盤面から同じ手を指して行き先が分かれるのは、盤面の違いで手の意味が変わるところ
function conditionalSteps(tree: PlaybookTree, stepsFrom: Map<string, PlaybookNodeStepDto[]>): Set<number> {
  const groups = new Map<string, PlaybookNodeStepDto[]>();
  for (const board of tree.boards.values()) {
    for (const step of stepsFrom.get(board.id) ?? []) {
      const key = `${board.nodeId}:${step.usis[0]}`;
      groups.set(key, [...(groups.get(key) ?? []), step]);
    }
  }
  const result = new Set<number>();
  for (const steps of groups.values()) {
    const targets = new Set(steps.map((s) => (s.toBoardId ? `node:${tree.boardsById.get(s.toBoardId)?.nodeId}` : `open:${s.id}`)));
    if (targets.size > 1) steps.forEach((s) => result.add(s.id));
  }
  return result;
}
