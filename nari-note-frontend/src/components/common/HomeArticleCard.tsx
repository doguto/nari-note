import Link from 'next/link';

interface HomeArticleCardProps {
  id: number;
  title: string;
  author: string;
  authorId: number;
  stats: string;
  date: string;
  image: string;
}

/**
 * ホームページ用記事カード
 * 
 * トップページの記事一覧で使用されるカードコンポーネント。
 * 著者名クリックでユーザーページに遷移できます。
 */
export function HomeArticleCard({
  id,
  title,
  author,
  authorId,
  stats,
  date,
  image,
}: HomeArticleCardProps) {
  return (
    <Link
      href={`/articles/${id}`}
      className="block bg-[#2d3e1f] rounded-lg p-4 text-white hover:bg-[#3d4e2f] transition-colors"
    >
      <div className="flex gap-4">
        <div className="w-24 h-24 bg-[#1a2515] rounded flex items-center justify-center text-4xl">
          {image}
        </div>
        <div className="flex-1">
          <h3 className="text-lg font-bold mb-2">{title}</h3>
          <div className="flex items-center gap-2 text-sm text-gray-300 mb-2">
            <span className="w-6 h-6 bg-gray-600 rounded-full"></span>
            <Link
              href={`/users/${authorId}`}
              onClick={(e) => e.stopPropagation()}
              className="hover:text-[#88b04b] hover:underline"
            >
              {author}
            </Link>
          </div>
          <div className="flex gap-4 text-sm text-gray-400">
            <span>{stats}</span>
            <span>{date}</span>
          </div>
        </div>
      </div>
    </Link>
  );
}
