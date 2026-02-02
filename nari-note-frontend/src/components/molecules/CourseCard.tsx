import Link from 'next/link';
import { Heart, BookOpen } from 'lucide-react';

interface CourseCardProps {
  id: number;
  name: string;
  userId: number;
  userName: string;
  articleCount: number;
  likeCount: number;
}

/**
 * CourseCard - Molecule Component
 * 
 * 講座カード表示コンポーネント
 * ホーム画面などで講座の概要を表示します
 */
export function CourseCard({
  id,
  name,
  userId,
  userName,
  articleCount,
  likeCount,
}: CourseCardProps) {
  return (
    <Link
      href={`/courses/${id}`}
      className="block bg-white rounded-lg p-5 border border-gray-200 hover:shadow-lg hover:border-brand-primary/30 transition-all duration-200"
    >
      <div className="flex flex-col gap-3">
        <h3 className="text-lg font-bold text-gray-800">{name}</h3>
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <span className="w-6 h-6 bg-gray-200 rounded-full" aria-hidden="true"></span>
          <Link
            href={`/users/${userId}`}
            onClick={(e) => e.stopPropagation()}
            className="hover:text-brand-primary hover:underline"
          >
            {userName}
          </Link>
        </div>
        <div className="flex gap-4 text-sm text-gray-600 items-center">
          <span className="flex items-center gap-1">
            <BookOpen className="w-4 h-4" />
            {articleCount}記事
          </span>
          <span className="flex items-center gap-1">
            <Heart className="w-4 h-4" />
            {likeCount}
          </span>
        </div>
      </div>
    </Link>
  );
}
