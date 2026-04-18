'use client';

import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Pencil, Trash2, MoreVertical } from 'lucide-react';

interface DraftArticleCardProps {
  id: string;
  title: string;
  tags: string[];
  updatedAt: string;
  onDelete: () => void;
}


export function DraftArticleCard({
  id,
  title,
  tags,
  updatedAt,
  onDelete,
}: DraftArticleCardProps) {
  const router = useRouter();

  const handleEdit = () => {
    router.push(`/articles/${id}/edit`);
  };

  const formattedDate = new Date(updatedAt).toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <div className="bg-white rounded-lg p-5 border border-gray-200 hover:shadow-lg hover:border-brand-primary/30 transition-all duration-200">
      <div className="flex flex-col gap-3">
        <div className="flex items-start justify-between gap-2">
          <h3
            onClick={handleEdit}
            className="text-lg font-bold text-gray-800 cursor-pointer hover:text-brand-primary transition-colors"
          >
            {title}
          </h3>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="sm" className="h-8 w-8 p-0 flex-shrink-0">
                <MoreVertical className="w-4 h-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={handleEdit}>
                <Pencil className="w-4 h-4 mr-2" />
                編集
              </DropdownMenuItem>
              <DropdownMenuItem onClick={onDelete} className="text-red-500 focus:text-red-500">
                <Trash2 className="w-4 h-4 mr-2" />
                削除
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        {tags.length > 0 && (
          <div className="flex gap-2 flex-wrap">
            {tags.map((tag) => (
              <span key={tag} className="bg-brand-primary/10 text-brand-primary rounded-full px-3 py-1 text-xs font-medium">
                #{tag}
              </span>
            ))}
          </div>
        )}
        <p className="text-sm text-gray-500">最終更新: {formattedDate}</p>
      </div>
    </div>
  );
}
