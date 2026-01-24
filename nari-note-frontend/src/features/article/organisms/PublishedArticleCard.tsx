'use client';

import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Pencil, Eye } from 'lucide-react';

interface PublishedArticleCardProps {
  id: number;
  title: string;
  publishedAt: string;
  likeCount?: number;
}

/**
 * PublishedArticleCard - Organism Component
 * 
 * 公開済み記事のカード表示コンポーネント
 * タイトル、公開日時、いいね数、表示・編集ボタンを表示
 */
export function PublishedArticleCard({
  id,
  title,
  publishedAt,
  likeCount = 0,
}: PublishedArticleCardProps) {
  const router = useRouter();

  const handleEdit = () => {
    router.push(`/articles/${id}/edit`);
  };

  const handleView = () => {
    router.push(`/articles/${id}`);
  };

  const formattedDate = new Date(publishedAt).toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          <h3 
            onClick={handleView}
            className="text-xl font-bold text-gray-900 mb-2 cursor-pointer hover:text-[var(--brand-primary)] transition-colors truncate"
          >
            {title}
          </h3>
          <div className="flex items-center gap-4 text-sm text-gray-500">
            <span>公開日: {formattedDate}</span>
            <span>いいね: {likeCount}</span>
          </div>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button
            onClick={handleView}
            variant="outline"
            size="sm"
            className="border-gray-300 text-gray-700 hover:bg-gray-50"
          >
            <Eye className="w-4 h-4 mr-1" />
            表示
          </Button>
          <Button
            onClick={handleEdit}
            variant="outline"
            size="sm"
            className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
          >
            <Pencil className="w-4 h-4 mr-1" />
            編集
          </Button>
        </div>
      </div>
    </div>
  );
}
