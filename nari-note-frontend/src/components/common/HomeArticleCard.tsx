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
      className="block bg-brand-text rounded-lg p-4 text-white hover:bg-brand-text-hover transition-colors"
    >
      <div className="flex gap-4">
        <div className="w-24 h-24 bg-brand-text-dark rounded flex items-center justify-center text-4xl">
          {image}
        </div>
        <div className="flex-1">
          <h3 className="text-lg font-bold mb-2">{title}</h3>
          <div className="flex items-center gap-2 text-sm text-gray-300 mb-2">
            <span className="w-6 h-6 bg-gray-600 rounded-full"></span>
            <Link
              href={`/users/${authorId}`}
              onClick={(e) => e.stopPropagation()}
              className="hover:text-brand-primary hover:underline"
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
