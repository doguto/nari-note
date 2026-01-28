'use client';

import { useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { LoadingSpinner } from '@/components/ui';
import { ArticleFormPage } from '@/features/article/pages';

/**
 * 記事編集ページ
 * 
 * 既存記事の編集機能
 */
export default function EditArticlePage() {
  const router = useRouter();
  const params = useParams();
  const { isLoggedIn, isLoading } = useAuth();
  const articleId = Number(params.id);

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

  return <ArticleFormPage articleId={articleId} mode="edit" />;
}
