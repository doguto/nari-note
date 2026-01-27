'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useGetArticlesByAuthor, useDeleteArticle } from '@/lib/api';
import { DraftArticleCard } from '@/components/molecules/DraftArticleCard';
import { PublishedArticleCard } from '@/components/molecules/PublishedArticleCard';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';

/**
 * MyArticlesListPage - Organism Component
 * 
 * マイ記事一覧ページの完全な機能を持つコンポーネント
 * 下書き記事と公開済み記事をタブで切り替えて表示
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
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

  const displayArticles = activeTab === 'published' ? publishedArticles : draftArticles;
  const emptyMessage = activeTab === 'published' 
    ? '公開済みの記事がありません' 
    : '下書きの記事がありません';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">マイ記事一覧</h1>
        <Button
          onClick={handleNewArticle}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          <Plus className="w-4 h-4 mr-2" />
          新規記事作成
        </Button>
      </div>

      {/* タブナビゲーション */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex gap-8 px-6">
            <button
              onClick={() => setActiveTab('published')}
              className={`py-4 border-b-2 ${
                activeTab === 'published'
                  ? 'border-[var(--brand-primary)] text-[var(--brand-text)] font-medium'
                  : 'border-transparent text-gray-600 hover:text-[var(--brand-text)]'
              }`}
            >
              公開済み ({publishedArticles.length})
            </button>
            <button
              onClick={() => setActiveTab('drafts')}
              className={`py-4 border-b-2 ${
                activeTab === 'drafts'
                  ? 'border-[var(--brand-primary)] text-[var(--brand-text)] font-medium'
                  : 'border-transparent text-gray-600 hover:text-[var(--brand-text)]'
              }`}
            >
              下書き ({draftArticles.length})
            </button>
          </nav>
        </div>

        {/* タブコンテンツ */}
        <div className="p-6">
          {displayArticles.length === 0 ? (
            <div className="text-center py-16">
              <p className="text-gray-500 text-lg mb-6">{emptyMessage}</p>
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
              {activeTab === 'published' ? (
                displayArticles.map((article) => (
                  <PublishedArticleCard
                    key={article.id}
                    id={article.id!}
                    title={article.title ?? '無題'}
                    publishedAt={article.publishedAt ?? ''}
                    likeCount={article.likeCount ?? 0}
                  />
                ))
              ) : (
                displayArticles.map((article) => (
                  <DraftArticleCard
                    key={article.id}
                    id={article.id!}
                    title={article.title ?? '無題'}
                    updatedAt={article.updatedAt ?? ''}
                    onDelete={() => handleDelete(article.id!, article.title ?? '無題')}
                  />
                ))
              )}
            </div>
          )}
        </div>
      </div>

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
