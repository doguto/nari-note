'use client';

import { useParams } from 'next/navigation';
import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';
import { ArticleFormPage } from '@/features/article/pages';

/**
 * 記事編集ページ
 * 
 * 既存記事の編集機能
 */
export default function EditArticlePage() {
  const params = useParams();
  const { isLoggedIn, isLoading } = useRequireAuth(`/articles/${params.id}/edit`);
  const articleId = Number(params.id);

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
