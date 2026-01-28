'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailPage } from '@/features/article/pages';

export default function ArticleDetailPageRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailPage articleId={articleId} />;
}
