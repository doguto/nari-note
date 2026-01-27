'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useGetArticlesByAuthor, useDeleteArticle } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { MyArticlesListTemplate } from '../templates/MyArticlesListTemplate';

/**
 * MyArticlesListPage - Page Component
 * 
 * マイ記事一覧ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function MyArticlesListPage() {
  const router = useRouter();
  const { userId } = useAuth();
  const [activeTab, setActiveTab] = useState<'published' | 'drafts'>('published');
  const [deletingId, setDeletingId] = useState<number | null>(null);
  
  const { data, isLoading, error, refetch } = useGetArticlesByAuthor(
    { authorId: userId || 0 },
    { enabled: !!userId }
  );
  
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
    return <LoadingSpinner text="記事を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  const allArticles = data?.articles || [];
  const publishedArticles = allArticles.filter(article => article.isPublished);
  const draftArticles = allArticles.filter(article => !article.isPublished);

  return (
    <MyArticlesListTemplate
      activeTab={activeTab}
      publishedArticles={publishedArticles}
      draftArticles={draftArticles}
      deletingId={deletingId}
      onTabChange={setActiveTab}
      onNewArticle={handleNewArticle}
      onDelete={handleDelete}
    />
  );
}
