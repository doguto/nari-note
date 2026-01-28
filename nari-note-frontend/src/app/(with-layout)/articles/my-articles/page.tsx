'use client';

import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';
import { MyArticlesListPage } from '@/features/article/pages';

/**
 * マイ記事一覧ページ
 * 
 * ログインユーザーの下書き記事と公開済み記事一覧を表示
 */
export default function MyArticlesPage() {
  const { isLoggedIn, isLoading } = useRequireAuth('/articles/my-articles');

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

  return <MyArticlesListPage />;
}
