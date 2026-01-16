'use client';

import Link from 'next/link';
import { useGetUserProfile } from '@/lib/api';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Button } from '@/components/ui/button';

interface UserProfilePageProps {
  userId: number;
}

/**
 * UserProfilePage - Organism Component
 * 
 * ユーザープロフィールページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function UserProfilePage({ userId }: UserProfilePageProps) {
  const { data: user, isLoading, error, refetch } = useGetUserProfile({ id: userId });
  const { userId: currentUserId } = useAuth();
  
  const isOwnProfile = currentUserId === userId;

  if (isLoading) {
    return <Loading text="ユーザー情報を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="ユーザー情報の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!user) {
    return <ErrorMessage message="ユーザーが見つかりません" />;
  }

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-start gap-6">
          <div className="w-24 h-24 bg-brand-primary rounded-full flex items-center justify-center text-white text-4xl font-bold flex-shrink-0">
            {user.username?.charAt(0).toUpperCase() || 'U'}
          </div>
          
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-brand-text mb-2">
              {user.username || 'Unknown User'}
            </h1>
            <p className="text-gray-600 mb-4">
              @{user.username || 'unknown'}
            </p>
            
            {user.bio && (
              <p className="text-gray-700 mb-4">
                {user.bio}
              </p>
            )}
            
            <div className="flex gap-6 text-sm text-gray-600">
              <div>
                <span className="font-bold text-brand-text">0</span>
                <span className="ml-1">記事</span>
              </div>
              <div>
                <span className="font-bold text-brand-text">0</span>
                <span className="ml-1">フォロワー</span>
              </div>
              <div>
                <span className="font-bold text-brand-text">0</span>
                <span className="ml-1">フォロー中</span>
              </div>
            </div>
          </div>
          
          {isOwnProfile ? (
            <Link href="/settings/profile">
              <Button variant="outline">
                プロフィール編集
              </Button>
            </Link>
          ) : (
            <button className="px-6 py-2 bg-brand-primary text-white rounded hover:bg-brand-primary-hover transition-colors">
              フォロー
            </button>
          )}
        </div>
      </div>
      
      {/* タブナビゲーション */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex gap-8 px-6">
            <Link 
              href="#articles" 
              className="py-4 border-b-2 border-brand-primary text-brand-text font-medium"
            >
              記事
            </Link>
            <Link 
              href="#likes" 
              className="py-4 text-gray-600 hover:text-brand-text"
            >
              いいね
            </Link>
            <Link 
              href="#following" 
              className="py-4 text-gray-600 hover:text-brand-text"
            >
              フォロー中のタグ
            </Link>
          </nav>
        </div>
      </div>
    </div>
  );
}
