'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useGetArticlesByAuthor, useDeleteArticle } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { PageWithSidebar } from '@/features/global/organisms';
import { DeleteConfirmModal } from '@/components/molecules/DeleteConfirmModal';
import { MyArticlesListTemplate } from '../templates/MyArticlesListTemplate';

export function MyArticlesListPage() {
  const router = useRouter();
  const { userId } = useAuth();
  const [activeTab, setActiveTab] = useState<'published' | 'drafts'>('published');
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<{ id: string; title: string } | null>(null);

  const { data, isLoading, error, refetch } = useGetArticlesByAuthor(
    { authorId: userId || '' },
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

  const handleDeleteRequest = (id: string, title: string) => {
    setPendingDelete({ id, title });
  };

  const handleConfirmDelete = () => {
    if (!pendingDelete) return;
    setDeletingId(pendingDelete.id);
    setPendingDelete(null);
    deleteArticle.mutate({ id: pendingDelete.id });
  };

  const handleCancelDelete = () => {
    setPendingDelete(null);
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
    <PageWithSidebar>
      <MyArticlesListTemplate
        activeTab={activeTab}
        publishedArticles={publishedArticles}
        draftArticles={draftArticles}
        deletingId={deletingId}
        onTabChange={setActiveTab}
        onNewArticle={handleNewArticle}
        onDelete={handleDeleteRequest}
      />
      <DeleteConfirmModal
        open={pendingDelete !== null}
        articleTitle={pendingDelete?.title ?? ''}
        onConfirm={handleConfirmDelete}
        onCancel={handleCancelDelete}
      />
    </PageWithSidebar>
  );
}
