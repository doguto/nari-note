import Link from 'next/link';
import { Heart } from 'lucide-react';

interface ArticleCardProps {
  id: number;
  title: string;
  author: string;
  authorId: number;
  tags: string[];
  likeCount: number;
  date: string;
}

/**
 * ArticleCard - Molecule Component
 * 
 * 記事カード表示コンポーネント
 * ホーム画面などで記事の概要を表示します
 */
export function ArticleCard({
  id,
  title,
  author,
  authorId,
  tags,
  likeCount,
  date,
}: ArticleCardProps) {
  return (
    <Link
      href={`/articles/${id}`}
      className="block bg-white rounded-lg p-5 border border-gray-200 hover:shadow-lg hover:border-brand-primary/30 transition-all duration-200"
    >
      <div className="flex flex-col gap-3">
        <h3 className="text-lg font-bold text-gray-800">{title}</h3>
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <span className="w-6 h-6 bg-gray-200 rounded-full"></span>
          <Link
            href={`/users/${authorId}`}
            onClick={(e) => e.stopPropagation()}
            className="hover:text-brand-primary hover:underline"
          >
            {author}
          </Link>
        </div>
        <div className='flex gap-2 flex-wrap'>
          {
            tags.map((tag) => (
              <span key={tag} className="bg-brand-primary/10 text-brand-primary rounded-full px-3 py-1 text-xs font-medium">
                #{tag}
              </span>
            ))
          }
        </div>
        <div className="flex gap-4 text-sm text-gray-500 items-center">
          <span className="flex items-center gap-1">
            <Heart className="w-4 h-4" />
            {likeCount}
          </span>
          <span>{date}</span>
        </div>
      </div>
    </Link>
  );
}
