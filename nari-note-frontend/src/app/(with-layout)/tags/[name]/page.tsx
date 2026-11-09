
import type { Metadata } from 'next';
import { buildPageMetadata } from '@/lib/metadata';
import { TagArticleListPage } from '@/features/tag/pages';

interface TagPageProps {
  params: Promise<{ name: string }>;
  searchParams: Promise<{ page?: string; tab?: string }>;
}

export async function generateMetadata({ params }: TagPageProps): Promise<Metadata> {
  const { name } = await params;
  const decodedName = decodeURIComponent(name);

  return buildPageMetadata({
    title: `#${decodedName}`,
    description: `「${decodedName}」タグの将棋記事・講座一覧`,
    path: `/tags/${name}`,
  });
}

export default async function TagPage({ params, searchParams }: TagPageProps) {
  const { name } = await params;
  const { page, tab } = await searchParams;
  const parsedPage = Number(page);
  const currentPage = Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1;

  const activeTab = tab === 'courses' ? 'courses' : 'articles';

  return <TagArticleListPage tagName={name} activeTab={activeTab} page={currentPage} />;
}
