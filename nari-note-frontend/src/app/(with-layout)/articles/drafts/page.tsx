'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Sidebar } from '@/components/layout/Sidebar';
import { DraftArticleListPage } from '@/features/article/organisms';
import { Loading } from '@/components/common/Loading';

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
        <Loading text="読み込み中..." />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 w-full">
      <div className="flex gap-8">
        <main className="flex-1">
          <DraftArticleListPage />
        </main>
        <Sidebar />
      </div>
    </div>
  );
}
