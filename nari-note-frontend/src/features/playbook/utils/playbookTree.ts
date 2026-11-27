import type {
  GetPlaybookResponse,
  PlaybookBoardDto,
  PlaybookNodeDto,
  PlaybookBoardTransitionDto,
} from '@/lib/api/types';
import { destinationOf, moveLabel, play } from './shogiRules';

export interface TreeMove {
  usi: string;
  child: string;
  // 辺の 1 手目のときだけ、その辺（条件を持つ）を指す
  transition?: PlaybookBoardTransitionDto;
}

export interface PathStep {
  sfen: string;
  usi: string;
  label: string;
}

// 定跡を 1 手ごとのグラフに展開したもの。辺の途中の局面は、遷移の手順を指し直して求める
export interface PlaybookTree {
  rootSfen: string;
  children: Map<string, TreeMove[]>;
  parents: Map<string, { sfen: string; usi: string }>;
  boards: Map<string, PlaybookBoardDto>;
  boardsById: Map<string, PlaybookBoardDto>;
  nodes: Map<number, PlaybookNodeDto>;
  transitions: PlaybookBoardTransitionDto[];
}

export function buildPlaybookTree(data: GetPlaybookResponse): PlaybookTree {
  const boardsById = new Map(data.boards.map((b) => [b.id, b]));
  const children = new Map<string, TreeMove[]>();
  for (const transition of data.transitions) {
    let sfen = boardsById.get(transition.fromBoardId)?.sfen;
    if (!sfen) continue;
    transition.usis.forEach((usi, i) => {
      const child = play(sfen!, usi);
      const list = children.get(sfen!) ?? [];
      list.push({ usi, child, transition: i === 0 ? transition : undefined });
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
    transitions: data.transitions,
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

// 辺の途中の局面が載っている辺。ノードの局面や、定跡に無い局面では undefined
export function transitionThrough(tree: PlaybookTree, sfen: string): PlaybookBoardTransitionDto | undefined {
  if (tree.boards.has(sfen)) return undefined;
  let cursor = sfen;
  while (!tree.boards.has(cursor)) {
    const parent = tree.parents.get(cursor);
    if (!parent) return undefined;
    if (tree.boards.has(parent.sfen)) {
      return tree.children.get(parent.sfen)?.find((m) => m.usi === parent.usi)?.transition;
    }
    cursor = parent.sfen;
  }
  return undefined;
}
