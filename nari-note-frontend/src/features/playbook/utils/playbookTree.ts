import type { GetPlaybookResponse, PlaybookBoardTransitionDto, PlaybookNodeDto } from '@/lib/api/types';
import { destinationOf, moveLabel, play } from './shogiRules';

export interface TreeBoard {
  id: string;
  nodeId?: number;
  sfen: string;
}

export interface TreeMove {
  usi: string;
  child: string;
  transition: PlaybookBoardTransitionDto;
}

export interface PathStep {
  sfen: string;
  usi: string;
  label: string;
}

// 定跡の局面を SFEN で引き、遷移を局面ごとの次の手として並べたもの
export interface PlaybookTree {
  rootSfen: string;
  children: Map<string, TreeMove[]>;
  parents: Map<string, { sfen: string; usi: string }>;
  boards: Map<string, TreeBoard>;
  boardsById: Map<string, TreeBoard>;
  nodes: Map<number, PlaybookNodeDto>;
  transitions: PlaybookBoardTransitionDto[];
}

// ノードでない局面は SFEN が返らないので、開始局面から遷移を指して求める
export function buildPlaybookTree(data: GetPlaybookResponse): PlaybookTree {
  const outgoing = new Map<string, PlaybookBoardTransitionDto[]>();
  for (const t of data.transitions) outgoing.set(t.fromBoardId, [...(outgoing.get(t.fromBoardId) ?? []), t]);
  const sfenById = new Map(data.boards.flatMap((b) => (b.sfen ? [[b.id, b.sfen] as const] : [])));

  const children = new Map<string, TreeMove[]>();
  const parents = new Map<string, { sfen: string; usi: string }>();
  const root = data.boards.find((b) => b.sfen === data.rootSfen);
  const queue = root ? [root.id] : [];
  const seen = new Set(queue);
  while (queue.length > 0) {
    const id = queue.shift()!;
    const sfen = sfenById.get(id)!;
    for (const t of outgoing.get(id) ?? []) {
      const child = sfenById.get(t.toBoardId) ?? play(sfen, t.usi);
      sfenById.set(t.toBoardId, child);
      children.set(sfen, [...(children.get(sfen) ?? []), { usi: t.usi, child, transition: t }]);
      if (seen.has(t.toBoardId)) continue;
      seen.add(t.toBoardId);
      parents.set(child, { sfen, usi: t.usi });
      queue.push(t.toBoardId);
    }
  }

  const boards = data.boards.flatMap((b) => {
    const sfen = sfenById.get(b.id);
    return sfen ? [{ id: b.id, nodeId: b.nodeId ?? undefined, sfen }] : [];
  });
  return {
    rootSfen: data.rootSfen,
    children,
    parents,
    boards: new Map(boards.map((b) => [b.sfen, b])),
    boardsById: new Map(boards.map((b) => [b.id, b])),
    nodes: new Map(data.nodes.map((n) => [n.id, n])),
    transitions: data.transitions,
  };
}

export function contains(tree: PlaybookTree, sfen: string): boolean {
  return tree.boards.has(sfen);
}

export function nodeIdOf(tree: PlaybookTree, sfen: string): number | undefined {
  return tree.boards.get(sfen)?.nodeId;
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

// ノードでない局面が載っている辺の、ノードから出る 1 本目の遷移
export function edgeThrough(tree: PlaybookTree, sfen: string): PlaybookBoardTransitionDto | undefined {
  let cursor = sfen;
  while (nodeIdOf(tree, cursor) === undefined) {
    const parent = tree.parents.get(cursor);
    if (!parent) return undefined;
    if (nodeIdOf(tree, parent.sfen) !== undefined) {
      return tree.children.get(parent.sfen)?.find((m) => m.usi === parent.usi)?.transition;
    }
    cursor = parent.sfen;
  }
  return undefined;
}
