import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FollowButton, FollowStats, LoadingSpinner, ErrorMessage, UserAvatar } from '@/components/ui';
import { UserListItem } from '@/components/molecules';
import { ArticleList } from '@/components/organisms';
import type { GetUserProfileResponse, GetArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface UserProfileTemplateProps {
  // ユーザー情報
  user: GetUserProfileResponse;
  isOwnProfile: boolean;
  
  // タブ状態
  activeTab: string;
  tabContext: 'content' | 'follow';
  
  // コンテンツデータ
  articlesData?: GetArticlesResponse;
  likedArticlesData?: GetArticlesResponse;
  followersData?: GetFollowersResponse;
  followingsData?: GetFollowingsResponse;
  
  // ローディング・エラー状態
  isArticlesLoading: boolean;
  isLikedArticlesLoading: boolean;
  isFollowersLoading: boolean;
  isFollowingsLoading: boolean;
  
  articlesError: Error | null;
  likedArticlesError: Error | null;
  followersError: Error | null;
  followingsError: Error | null;
  
  // フォローボタン状態
  isFollowPending: boolean;
  
  // イベントハンドラ
  onTabChange: (tab: string) => void;
  onArticlesClick: () => void;
  onFollowClick: () => void;
  onArticlesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}

/**
 * UserProfileTemplate - Template Component
 * 
 * ユーザープロフィールページのUI構成を担当するテンプレートコンポーネント
 * レイアウトとコンポーネント配置を定義
 */
export function UserProfileTemplate({
  user,
  isOwnProfile,
  activeTab,
  tabContext,
  articlesData,
  likedArticlesData,
  followersData,
  followingsData,
  isArticlesLoading,
  isLikedArticlesLoading,
  isFollowersLoading,
  isFollowingsLoading,
  articlesError,
  likedArticlesError,
  followersError,
  followingsError,
  isFollowPending,
  onTabChange,
  onArticlesClick,
  onFollowClick,
  onArticlesRetry,
  onLikedArticlesRetry,
  onFollowersRetry,
  onFollowingsRetry,
}: UserProfileTemplateProps) {
  return (
    <div className="space-y-6">
      {/* プロフィールカード */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-start gap-6">
          <UserAvatar 
            username={user.username || 'Unknown User'} 
            size="xl"
          />
          
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
                onClick={onArticlesClick}
                className="hover:opacity-70 transition-opacity cursor-pointer"
              >
                <span className="font-bold text-brand-text">{user.articleCount ?? 0}</span>
                <span className="ml-1">記事</span>
              </button>
              <FollowStats
                label="フォロワー"
                count={user.followerCount || 0}
                onClick={() => onTabChange('followers')}
              />
              <FollowStats
                label="フォロー中"
                count={user.followingCount || 0}
                onClick={() => onTabChange('followings')}
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
              onClick={onFollowClick}
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
                  onClick={() => onTabChange('articles')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'articles'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  記事
                </button>
                <button
                  onClick={() => onTabChange('likes')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'likes'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  いいね
                </button>
                <button
                  onClick={() => onTabChange('following-tags')}
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
                  onClick={() => onTabChange('followers')}
                  className={`py-4 border-b-2 ${
                    activeTab === 'followers'
                      ? 'border-brand-primary text-brand-text font-medium'
                      : 'border-transparent text-gray-600 hover:text-brand-text'
                  }`}
                >
                  フォロワー
                </button>
                <button
                  onClick={() => onTabChange('followings')}
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
            <ArticleList
              articles={articlesData?.articles}
              isLoading={isArticlesLoading}
              error={articlesError}
              onRetry={onArticlesRetry}
              emptyMessage="まだ記事がありません"
            />
          )}
          
          {activeTab === 'likes' && (
            <ArticleList
              articles={likedArticlesData?.articles}
              isLoading={isLikedArticlesLoading}
              error={likedArticlesError}
              onRetry={onLikedArticlesRetry}
              emptyMessage="いいねした記事がありません"
            />
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
                  <LoadingSpinner text="フォロワーを読み込み中..." />
                </div>
              )}
              
              {followersError && (
                <div className="py-4">
                  <ErrorMessage 
                    message="フォロワーの取得に失敗しました" 
                    onRetry={onFollowersRetry}
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
                  <LoadingSpinner text="フォロー中のユーザーを読み込み中..." />
                </div>
              )}
              
              {followingsError && (
                <div className="py-4">
                  <ErrorMessage 
                    message="フォロー中のユーザーの取得に失敗しました" 
                    onRetry={onFollowingsRetry}
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
