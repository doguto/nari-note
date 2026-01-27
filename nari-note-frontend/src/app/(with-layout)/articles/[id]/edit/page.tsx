'use client';

import { useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { FormPageLayout } from '@/components/molecules';
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

  return (
    <FormPageLayout 
      title="記事を編集"
      description="マークダウン形式で記事を編集できます。プレビュー機能を使用して、公開前に記事の見た目を確認できます。"
    >
      <ArticleFormPage articleId={articleId} mode="edit" />
    </FormPageLayout>
  );
}
