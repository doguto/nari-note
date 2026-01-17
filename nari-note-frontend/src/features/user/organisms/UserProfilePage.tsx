'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useGetUserProfile, useToggleFollow } from '@/lib/api';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Button } from '@/components/ui/button';
import { FollowButton } from '@/components/common/atoms';
import { FollowStats } from '@/components/common/atoms';
import { FollowersModal } from './FollowersModal';
import { FollowingsModal } from './FollowingsModal';

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
  const { mutate: toggleFollow, isPending: isFollowPending } = useToggleFollow({
    onSuccess: () => {
      // フォロー/フォロー解除成功時にプロフィールを再取得
      refetch();
    },
  });
  
  const [showFollowersModal, setShowFollowersModal] = useState(false);
  const [showFollowingsModal, setShowFollowingsModal] = useState(false);
  
  const isOwnProfile = currentUserId === userId;

  // フォローボタンクリックハンドラ
  const handleFollowClick = () => {
    if (isFollowPending) return;
    toggleFollow({ followingId: userId });
  };

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
              <FollowStats
                label="フォロワー"
                count={user.followerCount || 0}
                onClick={() => setShowFollowersModal(true)}
              />
              <FollowStats
                label="フォロー中"
                count={user.followingCount || 0}
                onClick={() => setShowFollowingsModal(true)}
              />
            </div>
          </div>
          
          {isOwnProfile ? (
            <Link href="/settings/profile">
              <Button variant="outline">
                プロフィール編集
              </Button>
            </Link>
          ) : (
            <FollowButton
              isFollowing={user.isFollowing || false}
              onClick={handleFollowClick}
              disabled={isFollowPending}
            />
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

      {/* フォロワーモーダル */}
      <FollowersModal
        userId={userId}
        isOpen={showFollowersModal}
        onClose={() => setShowFollowersModal(false)}
      />

      {/* フォロー中モーダル */}
      <FollowingsModal
        userId={userId}
        isOpen={showFollowingsModal}
        onClose={() => setShowFollowingsModal(false)}
      />
    </div>
  );
}
