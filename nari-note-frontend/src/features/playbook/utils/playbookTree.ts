import type {
  GetPlaybookResponse,
  PlaybookNodeBoardDto,
  PlaybookNodeDto,
  PlaybookNodeStepDto,
} from '@/lib/api/types';
import { destinationOf, moveLabel, play } from './shogiRules';

export interface TreeMove {
  usi: string;
  child: string;
  // 手順の 1 手目のときだけ、その手順（条件を持つ）を指す
  step?: PlaybookNodeStepDto;
}

export interface PathStep {
  sfen: string;
  usi: string;
  label: string;
}

// 定跡を 1 手ごとの木に展開したもの。途中の局面は手順を指し直して求める
export interface PlaybookTree {
  rootSfen: string;
  children: Map<string, TreeMove[]>;
  parents: Map<string, { sfen: string; usi: string }>;
  boards: Map<string, PlaybookNodeBoardDto>;
  boardsById: Map<string, PlaybookNodeBoardDto>;
  nodes: Map<number, PlaybookNodeDto>;
  steps: PlaybookNodeStepDto[];
}

export function buildPlaybookTree(data: GetPlaybookResponse): PlaybookTree {
  const boardsById = new Map(data.boards.map((b) => [b.id, b]));
  const children = new Map<string, TreeMove[]>();
  for (const step of data.steps) {
    let sfen = boardsById.get(step.fromBoardId)?.sfen;
    if (!sfen) continue;
    step.usis.forEach((usi, i) => {
      const child = play(sfen!, usi);
      const list = children.get(sfen!) ?? [];
      list.push({ usi, child, step: i === 0 ? step : undefined });
      children.set(sfen!, list);
      sfen = child;
    });
  }

  const parents = new Map<string, { sfen: string; usi: string }>();
  const queue = [data.rootSfen];
  const seen = new Set(queue);
  while (queue.length > 0) {
    const sfen = queue.shift()!;
    for (const m of children.get(sfen) ?? []) {
      if (seen.has(m.child)) continue;
      seen.add(m.child);
      parents.set(m.child, { sfen, usi: m.usi });
      queue.push(m.child);
    }
  }

  return {
    rootSfen: data.rootSfen,
    children,
    parents,
    boards: new Map(data.boards.map((b) => [b.sfen, b])),
    boardsById,
    nodes: new Map(data.nodes.map((n) => [n.id, n])),
    steps: data.steps,
  };
}

export function contains(tree: PlaybookTree, sfen: string): boolean {
  return sfen === tree.rootSfen || tree.parents.has(sfen);
}

// 開始局面からの手順。「同」を付けるため、直前の手の行き先を引き継いで記譜する
export function pathTo(tree: PlaybookTree, sfen: string): PathStep[] {
  const moves: { sfen: string; usi: string }[] = [];
  let cursor = sfen;
  while (cursor !== tree.rootSfen) {
    const parent = tree.parents.get(cursor);
    if (!parent) return [];
    moves.unshift(parent);
    cursor = parent.sfen;
  }
  let previousTo: number | undefined;
  return moves.map((m) => {
    const label = moveLabel(m.sfen, m.usi, previousTo);
    previousTo = destinationOf(m.usi);
    return { ...m, label };
  });
}

export function nodeOf(tree: PlaybookTree, sfen: string): PlaybookNodeDto | undefined {
  const board = tree.boards.get(sfen);
  return board ? tree.nodes.get(board.nodeId) : undefined;
}
