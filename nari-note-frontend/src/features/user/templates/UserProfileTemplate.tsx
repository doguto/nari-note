import { ProfileCard } from '../organisms/ProfileCard';
import { ProfileTabNav } from '../organisms/ProfileTabNav';
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
      <ProfileCard
        user={user}
        isOwnProfile={isOwnProfile}
        isFollowPending={isFollowPending}
        onArticlesClick={onArticlesClick}
        onTabChange={onTabChange}
        onFollowClick={onFollowClick}
      />

      <ProfileTabNav
        activeTab={activeTab}
        tabContext={tabContext}
        articlesData={articlesData}
        likedArticlesData={likedArticlesData}
        followersData={followersData}
        followingsData={followingsData}
        isArticlesLoading={isArticlesLoading}
        isLikedArticlesLoading={isLikedArticlesLoading}
        isFollowersLoading={isFollowersLoading}
        isFollowingsLoading={isFollowingsLoading}
        articlesError={articlesError}
        likedArticlesError={likedArticlesError}
        followersError={followersError}
        followingsError={followingsError}
        onTabChange={onTabChange}
        onArticlesRetry={onArticlesRetry}
        onLikedArticlesRetry={onLikedArticlesRetry}
        onFollowersRetry={onFollowersRetry}
        onFollowingsRetry={onFollowingsRetry}
      />
    </div>
  );
}
