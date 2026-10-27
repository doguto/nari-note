import type { CapturedPiece, ParsedBoard, PieceType } from '@/lib/kif-player/types';

export const OG_IMAGE_SIZE = { width: 1200, height: 630 };

const CELL_SIZE = 50;
const PIECE_SIZE = 44;
const COLORS = {
  background: '#f5f3e8',
  board: '#dcb35c',
  boardLine: '#3a3a3a',
  title: '#1a2515',
  text: '#2d3e1f',
  brand: '#88b04b',
};

interface ArticleOgImageProps {
  title: string;
  authorName: string;
  board: ParsedBoard;
  pieceImages: Partial<Record<PieceType, string>>;
}

export function formatCaptured(pieces: CapturedPiece[]): string {
  if (pieces.length === 0) return 'なし';
  return pieces.map((p) => (p.count > 1 ? `${p.type}${p.count}` : p.type)).join(' ');
}

/** satori で描画する記事の OG 画像（左に局面、右にタイトル） */
export function ArticleOgImage({ title, authorName, board, pieceImages }: ArticleOgImageProps) {
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
        <div style={{ display: 'flex', flexDirection: 'column', border: `3px solid ${COLORS.boardLine}` }}>
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
                  {piece && pieceImages[piece.type] && (
                    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
                    <img
                      src={pieceImages[piece.type]}
                      width={PIECE_SIZE}
                      height={PIECE_SIZE}
                      style={piece.owner === 'gote' ? { transform: 'rotate(180deg)' } : undefined}
                    />
                  )}
                </div>
              ))}
            </div>
          ))}
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

function CapturedRow({ mark, pieces }: { mark: string; pieces: CapturedPiece[] }) {
  return (
    <div style={{ display: 'flex', fontSize: 26, color: COLORS.text, height: 36, alignItems: 'center' }}>
      {`${mark} 持駒 ${formatCaptured(pieces)}`}
    </div>
  );
}
