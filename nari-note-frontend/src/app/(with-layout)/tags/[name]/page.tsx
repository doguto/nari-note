import { TagArticleListPage } from '@/features/tag/pages';
import { getArticlesByTag } from '@/lib/api/server';

interface TagPageProps {
  params: Promise<{ name: string }>;
}

export default async function TagPage({ params }: TagPageProps) {
  const { name } = await params;
  const tagName = decodeURIComponent(name);
  
  // サーバーサイドでデータをフェッチ
  const data = await getArticlesByTag({ tagName }).catch(() => ({ articles: [] }));
  const articles = data?.articles ?? [];

  return <TagArticleListPage tag={tagName} articles={articles} />;
}
