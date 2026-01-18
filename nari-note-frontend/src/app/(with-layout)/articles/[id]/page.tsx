'use client';

import { useParams } from 'next/navigation';
import { Sidebar } from '@/components/layout/Sidebar';
import { ArticleDetailPage } from '@/features/article/organisms';

export default function ArticleDetailPageRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 w-full">
      <div className="flex gap-8">
        <div className="flex-1">
          <ArticleDetailPage articleId={articleId} />
        </div>
        
        <Sidebar />
      </div>
    </div>
  );
}
