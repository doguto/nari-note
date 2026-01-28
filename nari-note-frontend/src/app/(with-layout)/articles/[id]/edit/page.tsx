'use client';

import { useParams } from 'next/navigation';
import { AuthGuard } from '@/components/molecules';
import { ArticleFormPage } from '@/features/article/pages';

/**
 * 記事編集ページ
 * 
 * 既存記事の編集機能
 */
export default function EditArticlePage() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <AuthGuard redirectPath={`/articles/${params.id}/edit`}>
      <ArticleFormPage articleId={articleId} mode="edit" />
    </AuthGuard>
  );
}
