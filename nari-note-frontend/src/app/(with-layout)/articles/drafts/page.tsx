'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { PageWithSidebar } from '@/features/global/organisms';
import { DraftArticleListPage } from '@/features/article/pages';
import { LoadingSpinner } from '@/components/ui';

/**
 * 下書き記事一覧ページ
 * 
 * ログインユーザーの下書き記事一覧を表示
 * 未ログイン時はログインページへリダイレクト
 */
export default function DraftsPage() {
  const router = useRouter();
  const { isLoggedIn, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isLoggedIn) {
      router.push('/login');
    }
  }, [isLoggedIn, isLoading, router]);

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

  return (
    <PageWithSidebar>
      <DraftArticleListPage />
    </PageWithSidebar>
  );
}
