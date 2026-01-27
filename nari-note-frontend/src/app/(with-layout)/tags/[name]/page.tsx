'use client';

import { useParams } from 'next/navigation';
import { PageWithSidebar } from '@/features/global/organisms';
import { TagArticleListPage } from '@/features/tag/pages';

export default function TagPage() {
  const params = useParams();
  const tagName = decodeURIComponent(params.name as string);

  return (
    <PageWithSidebar>
      <TagArticleListPage tag={tagName} />
    </PageWithSidebar>
  );
}
