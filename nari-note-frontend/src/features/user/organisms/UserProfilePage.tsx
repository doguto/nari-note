'use client';

import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useGetUserProfile, useToggleFollow, useGetFollowers, useGetFollowings } from '@/lib/api';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Button } from '@/components/ui/button';
import { FollowButton } from '@/components/common/atoms';
import { FollowStats } from '@/components/common/atoms';
import { UserListItem } from '@/components/common/molecules';

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
  const router = useRouter();
  const searchParams = useSearchParams();
  const activeTab = searchParams.get('tab') || 'articles';
  
  // タブコンテキストを判定（content系 or follow系）
  const tabContext = ['followers', 'followings'].includes(activeTab) ? 'follow' : 'content';
  
  const { data: user, isLoading, error, refetch } = useGetUserProfile({ id: userId });
  const { userId: currentUserId } = useAuth();
  const { mutate: toggleFollow, isPending: isFollowPending } = useToggleFollow({
    onSuccess: () => {
      // フォロー/フォロー解除成功時にプロフィールを再取得
      refetch();
    },
  });
  
  // フォロワー一覧取得（タブがfollowersの場合のみ）
  const { data: followersData, isLoading: isFollowersLoading, error: followersError, refetch: refetchFollowers } = useGetFollowers(
    { userId: userId },
    { enabled: activeTab === 'followers' }
  );
  
  // フォロー中一覧取得（タブがfollowingsの場合のみ）
  const { data: followingsData, isLoading: isFollowingsLoading, error: followingsError, refetch: refetchFollowings } = useGetFollowings(
    { userId: userId },
    { enabled: activeTab === 'followings' }
  );
  
  const isOwnProfile = currentUserId === userId;

  // フォローボタンクリックハンドラ
  const handleFollowClick = () => {
    if (isFollowPending) return;
    toggleFollow({ followingId: userId });
  };
  
  // タブ切り替えハンドラ
  const handleTabChange = (tab: string) => {
    router.push(`/users/${userId}?tab=${tab}`);
  };
  
  // 記事数クリックハンドラ（contentタブに切り替え）
  const handleArticlesClick = () => {
    router.push(`/users/${userId}?tab=articles`);
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
              <button
                onClick={handleArticlesClick}
                className="hover:opacity-70 transition-opacity cursor-pointer"
              >
                <span className="font-bold text-brand-text">0</span>
                <span className="ml-1">記事</span>
              </button>
              <FollowStats
                label="フォロワー"
                count={user.followerCount || 0}
                onClick={() => handleTabChange('followers')}
              />
              <FollowStats
                label="フォロー中"
                count={user.followingCount || 0}
                onClick={() => handleTabChange('followings')}
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
            {tabContext === 'content' ? (
              <>
                {/* コンテンツタブ（記事/いいね/フォロー中のタグ） */}
                <button
                  onClick={() => handleTabChange('articles')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'articles'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  記事
                </button>
                <button
                  onClick={() => handleTabChange('likes')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'likes'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  いいね
                </button>
                <button
                  onClick={() => handleTabChange('following-tags')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'following-tags'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  フォロー中のタグ
                </button>
              </>
            ) : (
              <>
                {/* フォロータブ（フォロワー/フォロー中） */}
                <button
                  onClick={() => handleTabChange('followers')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'followers'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  フォロワー
                </button>
                <button
                  onClick={() => handleTabChange('followings')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'followings'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  フォロー中
                </button>
              </>
            )}
          </nav>
        </div>
        
        {/* タブコンテンツ */}
        <div className="p-6">
          {activeTab === 'articles' && (
            <div className="text-center py-8 text-gray-500">
              記事一覧は今後実装予定です
            </div>
          )}
          
          {activeTab === 'likes' && (
            <div className="text-center py-8 text-gray-500">
              いいね一覧は今後実装予定です
            </div>
          )}
          
          {activeTab === 'following-tags' && (
            <div className="text-center py-8 text-gray-500">
              フォロー中のタグ一覧は今後実装予定です
            </div>
          )}
          
          {activeTab === 'followers' && (
            <div>
              {isFollowersLoading && (
                <div className="py-8">
                  <Loading text="フォロワーを読み込み中..." />
                </div>
              )}
              
              {followersError && (
                <div className="py-4">
                  <ErrorMessage 
                    message="フォロワーの取得に失敗しました" 
                    onRetry={refetchFollowers}
                  />
                </div>
              )}
              
              {!isFollowersLoading && !followersError && followersData?.followers && followersData.followers.length === 0 && (
                <div className="text-center py-8 text-gray-500">
                  フォロワーがいません
                </div>
              )}
              
              {!isFollowersLoading && !followersError && followersData?.followers && followersData.followers.length > 0 && (
                <div className="space-y-2">
                  {followersData.followers.map((follower) => (
                    follower.id && follower.username ? (
                      <UserListItem
                        key={follower.id}
                        userId={follower.id}
                        username={follower.username}
                        profileImage={follower.profileImage}
                      />
                    ) : null
                  ))}
                </div>
              )}
            </div>
          )}
          
          {activeTab === 'followings' && (
            <div>
              {isFollowingsLoading && (
                <div className="py-8">
                  <Loading text="フォロー中のユーザーを読み込み中..." />
                </div>
              )}
              
              {followingsError && (
                <div className="py-4">
                  <ErrorMessage 
                    message="フォロー中のユーザーの取得に失敗しました" 
                    onRetry={refetchFollowings}
                  />
                </div>
              )}
              
              {!isFollowingsLoading && !followingsError && followingsData?.followings && followingsData.followings.length === 0 && (
                <div className="text-center py-8 text-gray-500">
                  フォロー中のユーザーがいません
                </div>
              )}
              
              {!isFollowingsLoading && !followingsError && followingsData?.followings && followingsData.followings.length > 0 && (
                <div className="space-y-2">
                  {followingsData.followings.map((following) => (
                    following.id && following.username ? (
                      <UserListItem
                        key={following.id}
                        userId={following.id}
                        username={following.username}
                        profileImage={following.profileImage}
                      />
                    ) : null
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
