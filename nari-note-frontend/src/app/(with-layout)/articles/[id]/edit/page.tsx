'use client';


import { useParams } from 'next/navigation';
import { AuthGuard, VerifiedEmailGuard } from '@/features/global/organisms';
import { ArticleFormPage } from '@/features/article/pages';

/**
 * 記事編集ページ
 * 
 * 既存記事の編集機能
 */
export default function EditArticlePage() {
  const params = useParams();
  const articleId = params.id as string;

  return (
    <AuthGuard redirectPath={`/articles/${params.id}/edit`}>
      <VerifiedEmailGuard>
        <ArticleFormPage articleId={articleId} mode="edit" />
      </VerifiedEmailGuard>
    </AuthGuard>
  );
}
