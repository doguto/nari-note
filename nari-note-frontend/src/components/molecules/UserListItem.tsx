'use client';

import { useRouter } from 'next/navigation';
import { UserAvatar } from '@/components/ui';

interface UserListItemProps {
  userId: number;
  username: string;
  profileImage?: string;
}

/**
 * UserListItem - Molecule Component
 * 
 * ユーザーリストアイテム表示コンポーネント
 * ユーザー情報を一覧で表示する際に使用
 */
export function UserListItem({ userId, username, profileImage }: UserListItemProps) {
  const router = useRouter();

  const handleClick = () => {
    router.push(`/users/${userId}`);
  };

  return (
    <div
      onClick={handleClick}
      className="flex items-center gap-4 p-4 hover:bg-gray-50 cursor-pointer transition-colors rounded-lg"
    >
      {/* ユーザーアイコン */}
      <UserAvatar username={username} profileImage={profileImage} size="md" />
      
      {/* ユーザー情報 */}
      <div className="flex-1 min-w-0">
        <div className="font-bold text-brand-text truncate">
          {username}
        </div>
      </div>
    </div>
  );
}
