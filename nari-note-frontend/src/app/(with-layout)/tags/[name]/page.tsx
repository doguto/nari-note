import type { Metadata } from 'next';
import { TagArticleListPage } from '@/features/tag/pages';

interface TagPageProps {
  params: Promise<{ name: string }>;
}

export async function generateMetadata({ params }: TagPageProps): Promise<Metadata> {
  const { name } = await params;
  const decodedName = decodeURIComponent(name);

  return {
    title: `#${decodedName}`,
    description: `「${decodedName}」タグの将棋記事一覧`,
  };
}

export default async function TagPage({ params }: TagPageProps) {
  const { name } = await params;

  return <TagArticleListPage tagName={name} />;
}
