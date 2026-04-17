'use client';


import { AuthGuard } from '@/features/global/organisms';
import { DraftArticleListPage } from '@/features/article/pages';


export default function DraftsPage() {
  return (
    <AuthGuard redirectPath="/articles/drafts">
      <DraftArticleListPage />
    </AuthGuard>
  );
}
