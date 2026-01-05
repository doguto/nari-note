import type { GetArticleResponse } from '@/lib/api/types';
import Link from 'next/link';

interface ArticleDetailProps {
  article: GetArticleResponse;
}

/**
 * 記事詳細 - Presentational Component
 * 
 * 記事の詳細情報を表示するコンポーネント。
 */
export function ArticleDetail({ article }: ArticleDetailProps) {
  return (
    <article className="bg-white rounded-lg shadow-lg p-8">
      <h1 className="text-4xl font-bold text-[#2d3e1f] mb-6">
        {article.title}
      </h1>
      
      <div className="flex items-center gap-6 mb-8 pb-6 border-b border-gray-200">
        <Link href={`/users/${article.authorId}`} className="flex items-center gap-3 hover:opacity-80 transition-opacity">
          <div className="w-12 h-12 bg-[#88b04b] rounded-full flex items-center justify-center text-white font-bold">
            {article.authorName?.charAt(0).toUpperCase() || 'A'}
          </div>
          <div>
            <div className="font-medium text-[#2d3e1f]">
              {article.authorName || 'Unknown Author'}
            </div>
            <div className="text-sm text-gray-500">
              {article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
            </div>
          </div>
        </Link>
        
        <div className="flex items-center gap-4 ml-auto">
          <button className="flex items-center gap-2 px-4 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939] transition-colors">
            <span>❤️</span>
            <span>{article.likeCount}</span>
          </button>
          <button className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 transition-colors">
            ストック
          </button>
        </div>
      </div>
      
      <div className="prose max-w-none mb-8">
        <div className="text-gray-800 whitespace-pre-wrap leading-relaxed">
          {article.body}
        </div>
      </div>
      
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 flex-wrap pt-6 border-t border-gray-200">
          {article.tags.map((tag) => (
            <Link
              key={tag}
              href={`/tags/${tag}`}
              className="px-3 py-1 bg-[#f5f3e8] text-[#2d3e1f] rounded-full text-sm hover:bg-[#e8e4d0] transition-colors"
            >
              #{tag}
            </Link>
          ))}
        </div>
      )}
    </article>
  );
}
