import { TagArticleListPage } from '@/features/tag/pages';

interface TagPageProps {
  params: Promise<{ name: string }>;
}

export default async function TagPage({ params }: TagPageProps) {
  const { name } = await params;
  
  return <TagArticleListPage tagName={name} />;
}
