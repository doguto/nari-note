import { KOMA_LABEL_MAP, KOMA_SHAPE_POINTS, PROMOTED_PIECES } from '@/lib/kif-player/constants';
import type { CapturedPiece, ParsedBoard } from '@/lib/kif-player/types';
import { KOMA_GLYPH_PATHS } from '@/lib/og/komaGlyphs';

export const OG_IMAGE_SIZE = { width: 1200, height: 630 };

const CELL_SIZE = 50;
const PIECE_SIZE = 44;
const STAR_SIZE = 8;
const FRAME_WIDTH = 3;
const BOARD_SIZE = CELL_SIZE * 9;
const PIECE_OFFSET = (CELL_SIZE - PIECE_SIZE) / 2;
// globals.css の --color-shogi-* と揃える
const COLORS = {
  background: '#f5f3e8',
  board: '#e9c98a',
  boardLine: '#5a4320',
  koma: '#f6e2b3',
  komaEdge: '#8a6a3a',
  komaInk: '#2a1d0c',
  komaPromoted: '#b3261e',
  title: '#1a2515',
  text: '#2d3e1f',
  brand: '#88b04b',
};

interface ArticleOgImageProps {
  title: string;
  authorName: string;
  board: ParsedBoard;
}

export function formatCaptured(pieces: CapturedPiece[]): string {
  if (pieces.length === 0) return 'なし';
  return pieces.map((p) => (p.count > 1 ? `${p.type}${p.count}` : p.type)).join(' ');
}

/** satori で描画する記事の OG 画像（左に局面、右にタイトル） */
export function ArticleOgImage({ title, authorName, board }: ArticleOgImageProps) {
  return (
    <div
      style={{
        width: '100%',
        height: '100%',
        display: 'flex',
        padding: 40,
        backgroundColor: COLORS.background,
        fontFamily: 'Noto Sans JP',
      }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 8 }}>
        <CapturedRow mark="△" pieces={board.captured.gote} />
        <OgBoard board={board} />
        <CapturedRow mark="▲" pieces={board.captured.sente} />
      </div>

      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          paddingLeft: 56,
          paddingTop: 16,
          paddingBottom: 16,
        }}
      >
        <div
          style={{
            display: 'block',
            fontSize: 54,
            fontWeight: 700,
            lineHeight: 1.35,
            color: COLORS.title,
            lineClamp: 5,
            overflow: 'hidden',
          }}
        >
          {title}
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div style={{ display: 'flex', fontSize: 32, color: COLORS.text }}>{authorName}</div>
          <div style={{ display: 'flex', fontSize: 36, fontWeight: 700, color: COLORS.brand }}>なりノート</div>
        </div>
      </div>
    </div>
  );
}

// satori は要素・文字・svg ごとに描画コストがかかり Workers の CPU 上限を超えるため、
// 盤・罫線・星・駒を 1 枚の svg にまとめ、駒の文字も事前に書き出したパスで描く
function OgBoard({ board }: { board: ParsedBoard }) {
  const pieces = board.board.flatMap((row, ri) =>
    row.flatMap((piece, ci) => (piece ? [{ piece, x: ci * CELL_SIZE + PIECE_OFFSET, y: ri * CELL_SIZE + PIECE_OFFSET }] : [])),
  );
  const lines = Array.from({ length: 8 }, (_, i) => (i + 1) * CELL_SIZE);
  const komaScale = PIECE_SIZE / 60;
  const outerSize = BOARD_SIZE + FRAME_WIDTH * 2;

  return (
    <svg width={outerSize} height={outerSize} viewBox={`${-FRAME_WIDTH} ${-FRAME_WIDTH} ${outerSize} ${outerSize}`}>
      <rect
        x={-FRAME_WIDTH / 2}
        y={-FRAME_WIDTH / 2}
        width={BOARD_SIZE + FRAME_WIDTH}
        height={BOARD_SIZE + FRAME_WIDTH}
        fill={COLORS.board}
        stroke={COLORS.boardLine}
        strokeWidth={FRAME_WIDTH}
      />
      <path
        d={lines.map((p) => `M${p} 0V${BOARD_SIZE}M0 ${p}H${BOARD_SIZE}`).join('')}
        stroke={COLORS.boardLine}
        strokeWidth={1}
      />
      {[3, 6].flatMap((r) => [3, 6].map((c) => (
        <circle key={`${r}-${c}`} cx={CELL_SIZE * c} cy={CELL_SIZE * r} r={STAR_SIZE / 2} fill={COLORS.boardLine} />
      )))}
      {pieces.map(({ piece, x, y }) => (
        <g
          key={`${x}-${y}`}
          transform={`translate(${x} ${y}) scale(${komaScale})${piece.owner === 'gote' ? ' rotate(180 30 30)' : ''}`}
        >
          <polygon
            points={KOMA_SHAPE_POINTS}
            fill={COLORS.koma}
            stroke={COLORS.komaEdge}
            strokeWidth={2.5}
            strokeLinejoin="round"
          />
          <path
            d={KOMA_GLYPH_PATHS[KOMA_LABEL_MAP[piece.type]]}
            fill={PROMOTED_PIECES.has(piece.type) ? COLORS.komaPromoted : COLORS.komaInk}
          />
        </g>
      ))}
    </svg>
  );
}

function CapturedRow({ mark, pieces }: { mark: string; pieces: CapturedPiece[] }) {
  return (
    <div style={{ display: 'flex', fontSize: 26, color: COLORS.text, height: 36, alignItems: 'center' }}>
      {`${mark} 持駒 ${formatCaptured(pieces)}`}
    </div>
  );
}
