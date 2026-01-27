'use client';

import { useParams } from 'next/navigation';
import { PageWithSidebar } from '@/features/global/organisms';
import { ArticleDetailPage } from '@/features/article/pages';

export default function ArticleDetailPageRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <PageWithSidebar>
      <ArticleDetailPage articleId={articleId} />
    </PageWithSidebar>
  );
}
