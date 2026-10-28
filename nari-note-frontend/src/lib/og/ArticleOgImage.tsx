import { KOMA_LABEL_MAP, KOMA_SHAPE_POINTS, PROMOTED_PIECES } from '@/lib/kif-player/constants';
import type { CapturedPiece, ParsedBoard, Piece } from '@/lib/kif-player/types';

export const OG_IMAGE_SIZE = { width: 1200, height: 630 };
export const KOMA_FONT_FAMILY = 'Shippori Mincho';

const CELL_SIZE = 50;
const PIECE_SIZE = 44;
const STAR_SIZE = 8;
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
        <div style={{ position: 'relative', display: 'flex', flexDirection: 'column', border: `3px solid ${COLORS.boardLine}` }}>
          {board.board.map((row, ri) => (
            <div key={ri} style={{ display: 'flex' }}>
              {row.map((piece, ci) => (
                <div
                  key={ci}
                  style={{
                    width: CELL_SIZE,
                    height: CELL_SIZE,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: COLORS.board,
                    borderRight: ci < 8 ? `1px solid ${COLORS.boardLine}` : 'none',
                    borderBottom: ri < 8 ? `1px solid ${COLORS.boardLine}` : 'none',
                  }}
                >
                  {piece && <OgKoma piece={piece} />}
                </div>
              ))}
            </div>
          ))}
          {[3, 6].flatMap((r) => [3, 6].map((c) => (
            <div
              key={`${r}-${c}`}
              style={{
                position: 'absolute',
                left: CELL_SIZE * c - STAR_SIZE / 2,
                top: CELL_SIZE * r - STAR_SIZE / 2,
                width: STAR_SIZE,
                height: STAR_SIZE,
                borderRadius: STAR_SIZE / 2,
                backgroundColor: COLORS.boardLine,
              }}
            />
          )))}
        </div>
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

/** satori は svg 内の text を描画できないため、五角形の上に文字を重ねる */
function OgKoma({ piece }: { piece: Piece }) {
  return (
    <div
      style={{
        position: 'relative',
        display: 'flex',
        width: PIECE_SIZE,
        height: PIECE_SIZE,
        transform: piece.owner === 'gote' ? 'rotate(180deg)' : undefined,
      }}
    >
      <svg width={PIECE_SIZE} height={PIECE_SIZE} viewBox="0 0 60 60" style={{ position: 'absolute', left: 0, top: 0 }}>
        <polygon
          points={KOMA_SHAPE_POINTS}
          fill={COLORS.koma}
          stroke={COLORS.komaEdge}
          strokeWidth={2.5}
          strokeLinejoin="round"
        />
      </svg>
      <div
        style={{
          position: 'absolute',
          left: 0,
          top: 3,
          width: PIECE_SIZE,
          height: PIECE_SIZE,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontFamily: KOMA_FONT_FAMILY,
          fontSize: 22,
          fontWeight: 700,
          color: PROMOTED_PIECES.has(piece.type) ? COLORS.komaPromoted : COLORS.komaInk,
        }}
      >
        {KOMA_LABEL_MAP[piece.type]}
      </div>
    </div>
  );
}

function CapturedRow({ mark, pieces }: { mark: string; pieces: CapturedPiece[] }) {
  return (
    <div style={{ display: 'flex', fontSize: 26, color: COLORS.text, height: 36, alignItems: 'center' }}>
      {`${mark} 持駒 ${formatCaptured(pieces)}`}
    </div>
  );
}
