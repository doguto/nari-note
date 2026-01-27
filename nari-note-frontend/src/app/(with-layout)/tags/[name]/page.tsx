'use client';

import { useParams } from 'next/navigation';
import { Sidebar } from '@/features/global/organisms/Sidebar';
import { TagArticleListPage } from '@/features/tag/organisms';

export default function TagPage() {
  const params = useParams();
  const tagName = decodeURIComponent(params.name as string);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 w-full">
      <div className="flex gap-8">
        <div className="flex-1">
          <TagArticleListPage tag={tagName} />
        </div>
        
        <Sidebar />
      </div>
    </div>
  );
}
