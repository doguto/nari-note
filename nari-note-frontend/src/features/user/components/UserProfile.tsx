import type { GetUserResponse } from '@/lib/api/types';
import Link from 'next/link';

interface UserProfileProps {
  user: GetUserResponse;
}

/**
 * ユーザープロフィール - Presentational Component
 * 
 * ユーザーの詳細情報を表示するコンポーネント。
 */
export function UserProfile({ user }: UserProfileProps) {
  return (
    <div className="space-y-6">
      {/* プロフィールヘッダー */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-start gap-6">
          <div className="w-24 h-24 bg-[#88b04b] rounded-full flex items-center justify-center text-white text-4xl font-bold flex-shrink-0">
            {user.name.charAt(0).toUpperCase()}
          </div>
          
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-[#2d3e1f] mb-2">
              {user.name}
            </h1>
            <p className="text-gray-600 mb-4">
              @{user.email.split('@')[0]}
            </p>
            
            {user.bio && (
              <p className="text-gray-700 mb-4">
                {user.bio}
              </p>
            )}
            
            <div className="flex gap-6 text-sm text-gray-600">
              <div>
                <span className="font-bold text-[#2d3e1f]">{user.articleCount || 0}</span>
                <span className="ml-1">記事</span>
              </div>
              <div>
                <span className="font-bold text-[#2d3e1f]">{user.followerCount || 0}</span>
                <span className="ml-1">フォロワー</span>
              </div>
              <div>
                <span className="font-bold text-[#2d3e1f]">{user.followingCount || 0}</span>
                <span className="ml-1">フォロー中</span>
              </div>
            </div>
          </div>
          
          <button className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939] transition-colors">
            フォロー
          </button>
        </div>
      </div>
      
      {/* タブナビゲーション */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex gap-8 px-6">
            <Link 
              href="#articles" 
              className="py-4 border-b-2 border-[#88b04b] text-[#2d3e1f] font-medium"
            >
              記事
            </Link>
            <Link 
              href="#likes" 
              className="py-4 text-gray-600 hover:text-[#2d3e1f]"
            >
              いいね
            </Link>
            <Link 
              href="#following" 
              className="py-4 text-gray-600 hover:text-[#2d3e1f]"
            >
              フォロー中のタグ
            </Link>
          </nav>
        </div>
      </div>
    </div>
  );
}
