import type { Metadata } from 'next';
import { ArticleDetailPage } from '@/features/article/pages';
import { getArticleContent } from '@/lib/api/server';

interface ArticlePageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: ArticlePageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const article = await getArticleContent({ id: Number(id) });
    const plainText = article.body.replace(/<[^>]+>/g, '');
    const description = plainText.slice(0, 160);

    return {
      title: article.title,
      description,
      openGraph: {
        type: 'article',
        title: article.title,
        description,
        publishedTime: article.publishedAt,
        authors: [article.authorName],
        tags: article.tags,
      },
    };
  } catch {
    return {};
  }
}

export default async function ArticleDetailPageRoute({ params }: ArticlePageProps) {
  const { id } = await params;

  return <ArticleDetailPage articleId={Number(id)} />;
}
