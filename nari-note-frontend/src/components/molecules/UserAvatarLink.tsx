import Link from 'next/link';
import { UserAvatar } from '@/components/ui';

type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

interface UserAvatarLinkProps {
  userId: number;
  username: string;
  profileImage?: string;
  size?: AvatarSize;
  showUsername?: boolean;
  className?: string;
}

/**
 * UserAvatarLink - Molecule Component
 * 
 * UserAvatarにリンクとユーザー名を組み合わせたコンポーネント
 * ArticleCardやCourseCardなどで使用
 */
export function UserAvatarLink({
  userId,
  username,
  profileImage,
  size = 'sm',
  showUsername = true,
  className = '',
}: UserAvatarLinkProps) {
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <UserAvatar username={username} profileImage={profileImage} size={size} />
      {showUsername && (
        <Link
          href={`/users/${userId}`}
          onClick={(e) => e.stopPropagation()}
          className="hover:text-brand-primary hover:underline text-sm text-gray-600"
        >
          {username}
        </Link>
      )}
    </div>
  );
}
