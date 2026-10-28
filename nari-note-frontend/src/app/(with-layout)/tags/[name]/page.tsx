
import type { Metadata } from 'next';
import { buildPageMetadata } from '@/lib/metadata';
import { TagArticleListPage } from '@/features/tag/pages';

interface TagPageProps {
  params: Promise<{ name: string }>;
  searchParams: Promise<{ page?: string }>;
}

export async function generateMetadata({ params }: TagPageProps): Promise<Metadata> {
  const { name } = await params;
  const decodedName = decodeURIComponent(name);

  return buildPageMetadata({
    title: `#${decodedName}`,
    description: `「${decodedName}」タグの将棋記事一覧`,
    path: `/tags/${name}`,
  });
}

export default async function TagPage({ params, searchParams }: TagPageProps) {
  const { name } = await params;
  const { page } = await searchParams;
  const parsedPage = Number(page);
  const currentPage = Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1;

  return <TagArticleListPage tagName={name} page={currentPage} />;
}
