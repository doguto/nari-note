import type {
  GetPlaybookResponse,
  PlaybookBoardDto,
  PlaybookBoardTransitionDto,
  PlaybookNodeDto,
} from '@/lib/api/types';
import { destinationOf, moveLabel } from './shogiRules';

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
  boards: Map<string, PlaybookBoardDto>;
  boardsById: Map<string, PlaybookBoardDto>;
  nodes: Map<number, PlaybookNodeDto>;
}

export function buildPlaybookTree(data: GetPlaybookResponse): PlaybookTree {
  const boardsById = new Map(data.boards.map((b) => [b.id, b]));
  const children = new Map<string, TreeMove[]>();
  for (const transition of data.transitions) {
    const from = boardsById.get(transition.fromBoardId);
    const to = boardsById.get(transition.toBoardId);
    if (!from || !to) continue;
    const list = children.get(from.sfen) ?? [];
    list.push({ usi: transition.usi, child: to.sfen, transition });
    children.set(from.sfen, list);
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
  };
}

export function contains(tree: PlaybookTree, sfen: string): boolean {
  return tree.boards.has(sfen);
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
  const nodeId = tree.boards.get(sfen)?.nodeId;
  return nodeId != null ? tree.nodes.get(nodeId) : undefined;
}
