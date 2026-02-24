import Link from 'next/link';
import { UserAvatar } from '@/components/ui';

interface ArticleAuthorInfoProps {
  authorId: number;
  authorName: string;
  createdAt?: string;
}

/**
 * ArticleAuthorInfo - Molecule Component
 *
 * 記事の著者情報（アバター・名前・投稿日時）を表示するコンポーネント
 */
export function ArticleAuthorInfo({ authorId, authorName, createdAt }: ArticleAuthorInfoProps) {
  return (
    <Link
      href={`/users/${authorId}`}
      className="flex items-center gap-3 hover:opacity-80 transition-opacity"
    >
      <UserAvatar username={authorName} size="md" />
      <div>
        <div className="font-medium text-brand-text">{authorName}</div>
        <div className="text-sm text-gray-500">
          {createdAt ? new Date(createdAt).toLocaleDateString('ja-JP') : ''}
        </div>
      </div>
    </Link>
  );
}
