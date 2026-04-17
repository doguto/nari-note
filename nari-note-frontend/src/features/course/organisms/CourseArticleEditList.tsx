import Link from 'next/link';
import { BookOpen, Pencil, Plus } from 'lucide-react';
import { CourseArticleDto } from '@/lib/api/types';

interface CourseArticleEditListProps {
  articles: CourseArticleDto[];
  courseId: string;
}

export function CourseArticleEditList({ articles, courseId }: CourseArticleEditListProps) {
  if (!articles || articles.length === 0) {
    return (
      <div className="space-y-2">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-bold text-gray-800 flex items-center gap-2">
            <BookOpen className="w-5 h-5" />
            講座の記事一覧
          </h2>
          <Link
            href={`/articles/new?courseId=${courseId}`}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-brand-primary rounded-lg hover:opacity-90 transition-opacity"
          >
            <Plus className="w-4 h-4" />
            記事を追加
          </Link>
        </div>
        <div className="text-center py-8 text-gray-500">
          <BookOpen className="w-12 h-12 mx-auto mb-2 opacity-50" />
          <p>この講座にはまだ記事がありません</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold text-gray-800 flex items-center gap-2">
          <BookOpen className="w-5 h-5" />
          講座の記事一覧
        </h2>
        <Link
          href={`/articles/new?courseId=${courseId}`}
          className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-brand-primary rounded-lg hover:opacity-90 transition-opacity"
        >
          <Plus className="w-4 h-4" />
          記事を追加
        </Link>
      </div>
      <div className="space-y-2">
        {articles.map((article, index) => (
          <Link
            key={article.id}
            href={`/articles/${article.id}/edit`}
            className="flex items-center gap-3 bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md hover:border-brand-primary/30 transition-all duration-200"
          >
            <div className="flex-shrink-0 w-8 h-8 bg-brand-primary/10 rounded-full flex items-center justify-center text-brand-primary font-bold text-sm">
              {article.articleOrder ?? index + 1}
            </div>
            <div className="flex-1 min-w-0">
              <h3 className="text-base font-medium text-gray-800 truncate">
                {article.title || '無題の記事'}
              </h3>
            </div>
            <div className="flex items-center gap-2 flex-shrink-0">
              {!article.isPublished && (
                <span className="text-xs text-gray-400 bg-gray-100 px-2 py-0.5 rounded">下書き</span>
              )}
              <Pencil className="w-4 h-4 text-gray-400" />
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
