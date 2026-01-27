'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useGetDraftArticles, useDeleteArticle } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { DraftArticleListTemplate } from '../templates/DraftArticleListTemplate';

/**
 * DraftArticleListPage - Page Component
 * 
 * 下書き記事一覧ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function DraftArticleListPage() {
  const router = useRouter();
  const [deletingId, setDeletingId] = useState<number | null>(null);
  
  const { data, isLoading, error, refetch } = useGetDraftArticles({});
  
  const deleteArticle = useDeleteArticle({
    onSuccess: () => {
      setDeletingId(null);
      refetch();
    },
    onError: (error) => {
      console.error('記事の削除に失敗しました:', error);
      alert('記事の削除に失敗しました。もう一度お試しください。');
      setDeletingId(null);
    },
  });

  const handleDelete = (id: number, title: string) => {
    if (window.confirm(`「${title}」を削除してもよろしいですか？`)) {
      setDeletingId(id);
      deleteArticle.mutate({ id });
    }
  };

  const handleNewArticle = () => {
    router.push('/articles/new');
  };

  if (isLoading) {
    return <LoadingSpinner text="下書きを読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="下書きの取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  const articles = data?.articles || [];

  return (
    <DraftArticleListTemplate
      articles={articles}
      deletingId={deletingId}
      onNewArticle={handleNewArticle}
      onDelete={handleDelete}
    />
  );
}
