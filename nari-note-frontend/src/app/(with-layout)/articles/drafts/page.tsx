'use client';

import { AuthGuard } from '@/features/global/organisms';
import { DraftArticleListPage } from '@/features/article/pages';

/**
 * 下書き記事一覧ページ
 * 
 * ログインユーザーの下書き記事一覧を表示
 */
export default function DraftsPage() {
  return (
    <AuthGuard redirectPath="/articles/drafts">
      <DraftArticleListPage />
    </AuthGuard>
  );
}
