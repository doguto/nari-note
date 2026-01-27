import Link from 'next/link';
import { Heart } from 'lucide-react';

interface ArticleCardProps {
  id: number;
  title: string;
  author: string;
  authorId: number;
  stats: string;
  date: string;
  image: string; // 後方互換性のため残すが使用しない
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
  stats,
  date,
}: ArticleCardProps) {
  // statsから数値を抽出（例: "いいね 5" -> "5", "いいね" -> "0"）
  // 既存のテンプレートとの後方互換性のため、文字列から抽出
  const likeCount = stats.match(/\d+/)?.[0] ?? '0';

  return (
    <Link
      href={`/articles/${id}`}
      className="block bg-brand-text rounded-lg p-4 text-white hover:bg-brand-text-hover transition-colors"
    >
      <div className="flex flex-col gap-3">
        <h3 className="text-lg font-bold">{title}</h3>
        <div className="flex items-center gap-2 text-sm text-gray-300">
          <span className="w-6 h-6 bg-gray-600 rounded-full"></span>
          <Link
            href={`/users/${authorId}`}
            onClick={(e) => e.stopPropagation()}
            className="hover:text-brand-primary hover:underline"
          >
            {author}
          </Link>
        </div>
        <div className="flex gap-4 text-sm text-gray-400 items-center">
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
