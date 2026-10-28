
import type { Metadata } from 'next';
import { ArticleDetailPage } from '@/features/article/pages';
import { getArticleContent } from '@/lib/api/server';
import { buildPageMetadata, findFirstImageUrl, markdownToPlainText } from '@/lib/metadata';
import { findArticleBoard } from '@/lib/og/findArticleBoard';

interface ArticlePageProps {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ published?: string }>;
}

export async function generateMetadata({ params }: ArticlePageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const { article } = await getArticleContent({ id });
    // 盤面埋め込み > 棋譜埋め込み（いずれも局面を描いた画像を生成する） > 本文の画像 > サイト既定の画像
    const imageUrl = findArticleBoard(article)
      ? `/api/og/articles/${id}?v=${encodeURIComponent(article.updatedAt)}`
      : findFirstImageUrl(article.body);

    return buildPageMetadata({
      title: article.title,
      description: markdownToPlainText(article.body) || `${article.authorName}さんの将棋記事`,
      path: `/articles/${id}`,
      imageUrl,
      article: {
        publishedTime: article.publishedAt,
        authors: [article.authorName],
        tags: article.tags,
      },
    });
  } catch {
    return {};
  }
}

export default async function ArticleDetailPageRoute({ params, searchParams }: ArticlePageProps) {
  const { id } = await params;
  // 記事の公開直後は、書き手にシェアを促す案内を表示する
  const { published } = await searchParams;

  return <ArticleDetailPage articleId={id} justPublished={published === '1'} />;
}
