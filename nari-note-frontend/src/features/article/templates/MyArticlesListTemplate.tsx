import { DraftArticleCard } from '@/components/molecules/DraftArticleCard';
import { PublishedArticleCard } from '@/components/molecules/PublishedArticleCard';
import { LoadingSpinner } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import { ArticleThumbnailDto } from '@/lib/api/types';

interface MyArticlesListTemplateProps {
  activeTab: 'published' | 'drafts';
  publishedArticles: ArticleThumbnailDto[];
  draftArticles: ArticleThumbnailDto[];
  deletingId: string | null;
  onTabChange: (tab: 'published' | 'drafts') => void;
  onNewArticle: () => void;
  onDelete: (id: string, title: string) => void;
}


export function MyArticlesListTemplate({
  activeTab,
  publishedArticles,
  draftArticles,
  deletingId,
  onTabChange,
  onNewArticle,
  onDelete,
}: MyArticlesListTemplateProps) {
  const displayArticles = activeTab === 'published' ? publishedArticles : draftArticles;
  const emptyMessage = activeTab === 'published' 
    ? '公開済みの記事がありません' 
    : '下書きの記事がありません';

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-xl sm:text-3xl font-bold text-gray-900">マイ記事一覧</h1>
        <Button
          onClick={onNewArticle}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          <Plus className="w-4 h-4 mr-2" />
          新規記事作成
        </Button>
      </div>

      {/* タブナビゲーション */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex gap-6 sm:gap-8 px-4 sm:px-6 overflow-x-auto">
            <button
              onClick={() => onTabChange('published')}
              className={`py-4 border-b-2 whitespace-nowrap flex-shrink-0 ${
                activeTab === 'published'
                  ? 'border-[var(--brand-primary)] text-[var(--brand-text)] font-medium'
                  : 'border-transparent text-gray-600 hover:text-[var(--brand-text)]'
              }`}
            >
              公開済み ({publishedArticles.length})
            </button>
            <button
              onClick={() => onTabChange('drafts')}
              className={`py-4 border-b-2 whitespace-nowrap flex-shrink-0 ${
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
        <div className="p-4 sm:p-6">
          {displayArticles.length === 0 ? (
            <div className="text-center py-16">
              <p className="text-gray-500 text-lg mb-6">{emptyMessage}</p>
              <Button
                onClick={onNewArticle}
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
                    onDelete={() => onDelete(article.id!, article.title ?? '無題')}
                  />
                ))
              ) : (
                displayArticles.map((article) => (
                  <DraftArticleCard
                    key={article.id}
                    id={article.id!}
                    title={article.title ?? '無題'}
                    updatedAt={article.updatedAt ?? ''}
                    onDelete={() => onDelete(article.id!, article.title ?? '無題')}
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
