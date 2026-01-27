'use client';

import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Pencil, Trash2 } from 'lucide-react';

interface DraftArticleCardProps {
  id: number;
  title: string;
  updatedAt: string;
  onDelete: () => void;
}

/**
 * DraftArticleCard - Molecule Component
 * 
 * 下書き記事のカード表示コンポーネント
 * タイトル、最終更新日時、編集・削除ボタンを表示
 */
export function DraftArticleCard({
  id,
  title,
  updatedAt,
  onDelete,
}: DraftArticleCardProps) {
  const router = useRouter();

  const handleEdit = () => {
    router.push(`/articles/${id}/edit`);
  };

  const handleTitleClick = () => {
    router.push(`/articles/${id}/edit`);
  };

  const formattedDate = new Date(updatedAt).toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          <h3 
            onClick={handleTitleClick}
            className="text-xl font-bold text-gray-900 mb-2 cursor-pointer hover:text-[var(--brand-primary)] transition-colors truncate"
          >
            {title}
          </h3>
          <p className="text-sm text-gray-500">
            最終更新: {formattedDate}
          </p>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button
            onClick={handleEdit}
            variant="outline"
            size="sm"
            className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
          >
            <Pencil className="w-4 h-4 mr-1" />
            編集
          </Button>
          <Button
            onClick={onDelete}
            variant="outline"
            size="sm"
            className="border-red-500 text-red-500 hover:bg-red-50"
          >
            <Trash2 className="w-4 h-4 mr-1" />
            削除
          </Button>
        </div>
      </div>
    </div>
  );
}
