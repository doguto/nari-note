import Link from 'next/link';
import type { GetArticleResponse } from '@/lib/api/types';

interface ArticleCardProps {
  article: GetArticleResponse;
  onLike?: () => void;
}

/**
 * 記事カード - Presentational Component
 * 
 * 記事の概要を表示するカードコンポーネント。
 * 一覧表示で使用されます。
 */
export function ArticleCard({ article, onLike }: ArticleCardProps) {
  return (
    <Link href={`/articles/${article.id}`} className="block bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
      <h2 className="text-2xl font-bold text-[#2d3e1f] mb-2">
        {article.title}
      </h2>
      
      <div className="flex items-center gap-4 text-sm text-gray-600 mb-4">
        <Link
          href={`/users/${article.authorId}`}
          onClick={(e) => e.stopPropagation()}
          className="hover:text-[#88b04b] hover:underline"
        >
          著者: {article.authorName}
        </Link>
        <span>いいね: {article.likeCount}</span>
      </div>
      
      <p className="text-gray-700 mb-4 line-clamp-3">
        {article.body}
      </p>
      
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 flex-wrap mb-4">
          {article.tags.map((tag, index) => (
            <Link
              key={index}
              href={`/tags/${tag}`}
              onClick={(e) => e.stopPropagation()}
              className="px-2 py-1 bg-[#f5f3e8] text-[#2d3e1f] rounded text-sm hover:bg-[#e8e4d0] transition-colors"
            >
              #{tag}
            </Link>
          ))}
        </div>
      )}
      
      {onLike && (
        <button
          onClick={(e) => {
            e.preventDefault();
            onLike();
          }}
          className="px-4 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939] transition-colors"
        >
          いいね
        </button>
      )}
    </Link>
  );
}
