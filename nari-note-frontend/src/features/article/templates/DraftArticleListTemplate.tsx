'use client';

import { DraftArticleCard } from '@/components/molecules/DraftArticleCard';
import { LoadingSpinner } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import { ArticleDto } from '@/lib/api/types';

interface DraftArticleListTemplateProps {
  articles: ArticleDto[];
  deletingId: number | null;
  onNewArticle: () => void;
  onDelete: (id: number, title: string) => void;
}

/**
 * DraftArticleListTemplate - Template Component
 * 
 * 下書き記事一覧ページのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function DraftArticleListTemplate({
  articles,
  deletingId,
  onNewArticle,
  onDelete,
}: DraftArticleListTemplateProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">下書き記事</h1>
        <Button
          onClick={onNewArticle}
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
            onClick={onNewArticle}
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
              onDelete={() => onDelete(article.id!, article.title ?? '無題')}
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
