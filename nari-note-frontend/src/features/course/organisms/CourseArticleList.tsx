import Link from 'next/link';
import { BookOpen } from 'lucide-react';
import { CourseArticleDto } from '@/lib/api/types';

interface CourseArticleListProps {
  articles: CourseArticleDto[];
}

/**
 * CourseArticleList - Organism Component
 * 
 * 講座に含まれる記事の一覧を表示するコンポーネント
 * 記事のタイトル、順序、公開状態を表示します
 */
export function CourseArticleList({ articles }: CourseArticleListProps) {
  if (!articles || articles.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <BookOpen className="w-12 h-12 mx-auto mb-2 opacity-50" />
        <p>この講座にはまだ記事がありません</p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
        <BookOpen className="w-5 h-5" />
        講座の記事一覧
      </h2>
      <div className="space-y-2">
        {articles.map((article, index) => (
          <Link
            key={article.id}
            href={`/articles/${article.id}`}
            className="block bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md hover:border-brand-primary/30 transition-all duration-200"
          >
            <div className="flex items-center gap-3">
              <div className="flex-shrink-0 w-8 h-8 bg-brand-primary/10 rounded-full flex items-center justify-center text-brand-primary font-bold text-sm">
                {article.articleOrder ?? index + 1}
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="text-base font-medium text-gray-800 truncate">
                  {article.title || '無題の記事'}
                </h3>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
