export interface LayoutNode {
  id: number;
  label: string;
  positionCount: number;
}

export interface LayoutEdge {
  from: number;
  to: number;
  labelWidth: number;
}

export interface PlacedNode<N extends LayoutNode> {
  node: N;
  x: number;
  y: number;
  width: number;
}

export interface PlacedEdge<E extends LayoutEdge> {
  edge: E;
  path: string;
  // 手のラベルを並べる縦の通り道
  lane: { x: number; top: number; bottom: number };
}

export interface GraphLayout<N extends LayoutNode, E extends LayoutEdge> {
  nodes: PlacedNode<N>[];
  edges: PlacedEdge<E>[];
  width: number;
  height: number;
}

export const NODE_HEIGHT = 30;
const CHAR_WIDTH = 13;
const COUNT_WIDTH = 26;
const MIN_SLOT = 120;
const SLOT_GAP = 24;
const LAYER_HEIGHT = 168;
const PADDING = 24;
const ELBOW = 22;

function nodeWidth(node: LayoutNode): number {
  if (!node.label) return 0;
  return node.label.length * CHAR_WIDTH + 24 + (node.positionCount > 1 ? COUNT_WIDTH : 0);
}

// 開始からのつながりの段数で上から下へ並べる。段の中は親の並び順の平均で決める
export function layoutGraph<N extends LayoutNode, E extends LayoutEdge>(nodes: N[], edges: E[], rootId: number): GraphLayout<N, E> {
  const outgoing = new Map<number, number[]>();
  for (const e of edges) {
    const list = outgoing.get(e.from) ?? [];
    list.push(e.to);
    outgoing.set(e.from, list);
  }

  const layer = new Map<number, number>([[rootId, 0]]);
  const queue = [rootId];
  while (queue.length > 0) {
    const id = queue.shift()!;
    for (const to of outgoing.get(id) ?? []) {
      if (layer.has(to)) continue;
      layer.set(to, layer.get(id)! + 1);
      queue.push(to);
    }
  }

  const rows: N[][] = [];
  for (const node of nodes) {
    const l = layer.get(node.id);
    if (l === undefined) continue;
    (rows[l] ??= []).push(node);
  }

  const order = new Map<number, number>();
  rows.forEach((row, l) => {
    if (l > 0) {
      const parentOrder = (id: number) => {
        const parents = edges.filter((e) => e.to === id && (layer.get(e.from) ?? Infinity) < l).map((e) => order.get(e.from) ?? 0);
        return parents.length ? parents.reduce((a, b) => a + b, 0) / parents.length : 0;
      };
      row.sort((a, b) => parentOrder(a.id) - parentOrder(b.id));
    }
    row.forEach((n, i) => order.set(n.id, i));
  });

  // 辺のラベル（もし＋手順）が隣と重ならないよう、行き先のノードの横幅に含める
  const incomingLabel = new Map<number, number>();
  // 手のラベルは出元の真下か行き先の真上に来るので、両方の横幅に含める
  for (const e of edges) {
    incomingLabel.set(e.to, Math.max(incomingLabel.get(e.to) ?? 0, e.labelWidth));
    incomingLabel.set(e.from, Math.max(incomingLabel.get(e.from) ?? 0, e.labelWidth));
  }
  const slotOf = (n: N) => Math.max(MIN_SLOT, nodeWidth(n), incomingLabel.get(n.id) ?? 0);
  const rowWidths = rows.map((row) => row.reduce((sum, n) => sum + slotOf(n) + SLOT_GAP, -SLOT_GAP));
  const contentWidth = Math.max(MIN_SLOT, ...rowWidths);

  const placed: PlacedNode<N>[] = [];
  rows.forEach((row, l) => {
    let cursor = PADDING + (contentWidth - rowWidths[l]) / 2;
    for (const node of row) {
      const slot = slotOf(node);
      placed.push({ node, width: nodeWidth(node), x: cursor + slot / 2, y: PADDING + NODE_HEIGHT / 2 + l * LAYER_HEIGHT });
      cursor += slot + SLOT_GAP;
    }
  });

  const byId = new Map(placed.map((p) => [p.node.id, p]));
  const siblings = new Map<string, E[]>();
  const fanOut = new Map<number, number>();
  const fanIn = new Map<number, number>();
  for (const e of edges) {
    const key = `${e.from}-${e.to}`;
    siblings.set(key, [...(siblings.get(key) ?? []), e]);
    fanOut.set(e.from, (fanOut.get(e.from) ?? 0) + 1);
    fanIn.set(e.to, (fanIn.get(e.to) ?? 0) + 1);
  }

  const placedEdges: PlacedEdge<E>[] = [];
  for (const edge of edges) {
    const a = byId.get(edge.from);
    const b = byId.get(edge.to);
    if (!a || !b) continue;
    const group = siblings.get(`${edge.from}-${edge.to}`)!;
    const offset = group.indexOf(edge) - (group.length - 1) / 2;
    const spread = Math.max(...group.map((e) => e.labelWidth)) + SLOT_GAP;
    const x1 = a.x;
    const y1 = a.y + NODE_HEIGHT / 2;
    const x2 = b.x;
    const y2 = b.width > 0 ? b.y - NODE_HEIGHT / 2 : b.y;

    if (b.y > a.y) {
      // 辺ごとに縦の通り道を 1 本持たせ、手のラベルはその上に並べる。
      // 枝分かれする辺は行き先の真上、合流する辺は出元の真下を通ると互いに重ならない
      const merging = (fanIn.get(edge.to) ?? 0) > 1 && group.length === 1;
      const lane = (merging ? x1 : x2) + (group.length > 1 ? offset * spread + (x1 + x2) / 2 - x2 : 0);
      const top = y1 + ELBOW;
      const bottom = y2 - ELBOW;
      placedEdges.push({
        edge,
        path: `M ${x1} ${y1} C ${x1} ${y1 + ELBOW * 0.6}, ${lane} ${top - ELBOW * 0.6}, ${lane} ${top} L ${lane} ${bottom} C ${lane} ${bottom + ELBOW * 0.6}, ${x2} ${y2 - ELBOW * 0.6}, ${x2} ${y2}`,
        lane: { x: lane, top, bottom },
      });
    } else {
      // 手順前後で浅い段へ合流する辺は、横に膨らませて描く
      const right = Math.max(a.x + a.width / 2, b.x + b.width / 2) + 48 + offset * spread;
      placedEdges.push({
        edge,
        path: `M ${a.x + a.width / 2} ${a.y} L ${right} ${a.y} L ${right} ${b.y} L ${b.x + b.width / 2} ${b.y}`,
        lane: { x: right, top: a.y, bottom: b.y },
      });
    }
  }

  const maxRight = Math.max(
    0,
    ...placed.map((p) => p.x + p.width / 2),
    ...placedEdges.map((e) => e.lane.x + e.edge.labelWidth / 2),
  );
  return {
    nodes: placed,
    edges: placedEdges,
    width: Math.max(PADDING * 2 + contentWidth, maxRight + PADDING),
    height: PADDING * 2 + NODE_HEIGHT + Math.max(0, rows.length - 1) * LAYER_HEIGHT,
  };
}
