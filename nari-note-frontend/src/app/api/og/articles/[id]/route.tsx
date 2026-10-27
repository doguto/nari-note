import { ImageResponse } from 'next/og';
import type { NextRequest } from 'next/server';
import { getArticleContent } from '@/lib/api/server';
import { KOMA_IMAGE_MAP } from '@/lib/kif-player/constants';
import type { PieceType } from '@/lib/kif-player/types';
import { findArticleBoard } from '@/lib/og/findArticleBoard';
import { ArticleOgImage, OG_IMAGE_SIZE, formatCaptured } from '@/lib/og/ArticleOgImage';
import { loadGoogleFont, loadPublicAsset, toDataUri } from '@/lib/og/assets';

// /api/[...path] のバックエンドプロキシより優先され、ここだけは Next.js 側で処理する。
// URL に記事の更新日時（?v=）を付けて参照されるため、同じ URL の内容は変わらない前提で長くキャッシュする
const CACHE_CONTROL = 'public, max-age=86400, s-maxage=604800';
const FONT_FAMILY = 'Noto Sans JP';

export async function GET(request: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  let article;
  try {
    ({ article } = await getArticleContent({ id }));
  } catch {
    return new Response(null, { status: 404 });
  }

  const board = findArticleBoard(article);
  if (!board) return new Response(null, { status: 404 });

  const pieceTypes = new Set<PieceType>();
  board.board.flat().forEach((piece) => piece && pieceTypes.add(piece.type));

  const text = [
    article.title,
    article.authorName,
    'なりノート△▲持駒なし0123456789 ',
    formatCaptured(board.captured.sente),
    formatCaptured(board.captured.gote),
  ].join('');

  const [font, ...images] = await Promise.all([
    loadGoogleFont(FONT_FAMILY, 700, text),
    ...[...pieceTypes].map(async (type) => {
      const data = await loadPublicAsset(`/koma/${KOMA_IMAGE_MAP[type]}.png`, request.url);
      return [type, data ? toDataUri(data, 'image/png') : undefined] as const;
    }),
  ]);

  return new ImageResponse(
    <ArticleOgImage
      title={article.title}
      authorName={article.authorName}
      board={board}
      pieceImages={Object.fromEntries(images)}
    />,
    {
      ...OG_IMAGE_SIZE,
      fonts: font ? [{ name: FONT_FAMILY, data: font, weight: 700, style: 'normal' }] : undefined,
      headers: { 'Cache-Control': CACHE_CONTROL },
    },
  );
}
