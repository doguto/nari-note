import type { Metadata } from 'next';

export const SITE_NAME = 'なりノート';
export const SITE_TITLE = '将棋ブログ投稿サイト ～なりノート～';
export const SITE_DESCRIPTION = '将棋の記事投稿プラットフォーム。みんなの記事を読んで、将棋の知識を深めよう！';

// app/opengraph-image.jpg（ページ側で openGraph を指定すると継承されないため明示する）
const DEFAULT_OG_IMAGE = '/opengraph-image.jpg';
const DESCRIPTION_MAX_LENGTH = 120;

interface PageMetadataOptions {
  title: string;
  description: string;
  path: string;
  imageUrl?: string;
  // ユーザーアイコンのような正方形の画像は、X で大きな横長カードにすると見切れるため小さいカードにする
  isSquareImage?: boolean;
  article?: {
    publishedTime?: string;
    authors?: string[];
    tags?: string[];
  };
  profile?: {
    username: string;
  };
}

/**
 * ページごとの metadata を組み立てる。
 * Next.js は openGraph / twitter をオブジェクトごと上書きするため、ルートの設定（サイト名・画像等）もここで補う
 */
export function buildPageMetadata({
  title,
  description,
  path,
  imageUrl,
  isSquareImage = false,
  article,
  profile,
}: PageMetadataOptions): Metadata {
  const trimmedDescription = truncate(description, DESCRIPTION_MAX_LENGTH);
  const images = [imageUrl ?? DEFAULT_OG_IMAGE];
  const common = {
    siteName: SITE_NAME,
    locale: 'ja_JP',
    url: path,
    title,
    description: trimmedDescription,
    images,
  };

  const openGraph: Metadata['openGraph'] = article
    ? { ...common, type: 'article', ...article }
    : profile
      ? { ...common, type: 'profile', ...profile }
      : { ...common, type: 'website' };

  return {
    title,
    description: trimmedDescription,
    alternates: { canonical: path },
    openGraph,
    twitter: {
      card: imageUrl && isSquareImage ? 'summary' : 'summary_large_image',
      title,
      description: trimmedDescription,
      images,
    },
  };
}

/** 記事本文（Markdown）から説明文用の平文を作る */
export function markdownToPlainText(markdown: string): string {
  return markdown
    .replace(/^```[\s\S]*?^```[ \t]*$/gm, ' ') // コードブロック（棋譜・盤面の埋め込みを含む）
    .replace(/!\[[^\]]*\]\([^)]*\)/g, ' ') // 画像
    .replace(/\[([^\]]*)\]\([^)]*\)/g, '$1') // リンクは表示テキストだけ残す
    .replace(/<[^>]+>/g, '')
    .replace(/^[ \t]*(?:#{1,6}|>|[-*+]|\d+\.)[ \t]+/gm, '') // 見出し・引用・リストの記号
    .replace(/^[ \t]*(?:-{3,}|\*{3,}|_{3,})[ \t]*$/gm, ' ') // 水平線
    .replace(/\*\*|__|~~|`|\*/g, '')
    .replace(/\s+/g, ' ')
    .trim();
}

/** 本文中の最初の画像（https のみ）の URL を返す */
export function findFirstImageUrl(markdown: string): string | undefined {
  return markdown.match(/!\[[^\]]*\]\((https:\/\/[^)\s]+)/)?.[1];
}

function truncate(text: string, maxLength: number): string {
  return text.length > maxLength ? `${text.slice(0, maxLength)}…` : text;
}
