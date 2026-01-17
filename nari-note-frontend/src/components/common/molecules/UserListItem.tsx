'use client';

import { useRouter } from 'next/navigation';

interface UserListItemProps {
  userId: number;
  username: string;
  bio?: string;
}

/**
 * UserListItem - Molecule Component
 * 
 * ユーザーリストアイテム
 * ユーザーアイコン、ユーザー名、bioを表示
 * クリックでユーザープロフィールページに遷移
 */
export function UserListItem({ userId, username, bio }: UserListItemProps) {
  const router = useRouter();

  const handleClick = () => {
    router.push(`/users/${userId}`);
  };

  return (
    <div
      onClick={handleClick}
      className="flex items-start gap-4 p-4 hover:bg-gray-50 cursor-pointer transition-colors rounded-lg"
    >
      {/* ユーザーアイコン */}
      <div className="w-12 h-12 bg-brand-primary rounded-full flex items-center justify-center text-white text-lg font-bold flex-shrink-0">
        {username.charAt(0).toUpperCase()}
      </div>
      
      {/* ユーザー情報 */}
      <div className="flex-1 min-w-0">
        <div className="font-bold text-brand-text truncate">
          {username}
        </div>
        {bio && (
          <div className="text-sm text-gray-600 truncate">
            {bio}
          </div>
        )}
      </div>
    </div>
  );
}
