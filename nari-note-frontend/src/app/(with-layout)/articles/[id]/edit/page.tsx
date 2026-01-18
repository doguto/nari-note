'use client';

import { useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { ArticleFormPage } from '@/features/article/organisms';

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
        <div className="text-gray-500">読み込み中...</div>
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 w-full">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-[var(--brand-text)] mb-2">
          記事を編集
        </h1>
        <p className="text-gray-600">
          マークダウン形式で記事を編集できます。プレビュー機能を使用して、公開前に記事の見た目を確認できます。
        </p>
      </div>
      
      <div className="bg-white rounded-lg shadow-lg p-8">
        <ArticleFormPage articleId={articleId} mode="edit" />
      </div>
    </div>
  );
}
