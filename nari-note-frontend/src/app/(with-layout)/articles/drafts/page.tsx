'use client';

import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';
import { DraftArticleListPage } from '@/features/article/pages';

/**
 * 下書き記事一覧ページ
 * 
 * ログインユーザーの下書き記事一覧を表示
 */
export default function DraftsPage() {
  const { isLoggedIn, isLoading } = useRequireAuth('/articles/drafts');

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <LoadingSpinner text="読み込み中..." />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return <DraftArticleListPage />;
}
