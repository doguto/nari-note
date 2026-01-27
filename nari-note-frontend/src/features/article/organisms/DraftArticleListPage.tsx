'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useGetDraftArticles, useDeleteArticle } from '@/lib/api';
import { DraftArticleCard } from '@/components/molecules/DraftArticleCard';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';

/**
 * DraftArticleListPage - Organism Component
 * 
 * 下書き記事一覧ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
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
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">下書き記事</h1>
        <Button
          onClick={handleNewArticle}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          <Plus className="w-4 h-4 mr-2" />
          新規記事作成
        </Button>
      </div>

      {articles.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-gray-500 text-lg mb-6">下書きがありません</p>
          <Button
            onClick={handleNewArticle}
            className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            <Plus className="w-4 h-4 mr-2" />
            新規記事を作成
          </Button>
        </div>
      ) : (
        <div className="space-y-4">
          {articles.map((article) => (
            <DraftArticleCard
              key={article.id}
              id={article.id!}
              title={article.title ?? '無題'}
              updatedAt={article.updatedAt ?? ''}
              onDelete={() => handleDelete(article.id!, article.title ?? '無題')}
            />
          ))}
        </div>
      )}

      {deletingId !== null && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg">
            <LoadingSpinner text="削除中..." />
          </div>
        </div>
      )}
    </div>
  );
}
