import { ProfileCard } from '../organisms/ProfileCard';
import { ProfileTabNav } from '../organisms/ProfileTabNav';
import type { GetUserProfileResponse, GetArticlesByAuthorResponse, GetLikedArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface UserProfileTemplateProps {
  user: GetUserProfileResponse;
  isOwnProfile: boolean;

  activeTab: string;
  tabContext: 'content' | 'follow';

  articlesData?: GetArticlesByAuthorResponse;
  likedArticlesData?: GetLikedArticlesResponse;
  followersData?: GetFollowersResponse;
  followingsData?: GetFollowingsResponse;

  isArticlesLoading: boolean;
  isLikedArticlesLoading: boolean;
  isFollowersLoading: boolean;
  isFollowingsLoading: boolean;

  articlesError: Error | null;
  likedArticlesError: Error | null;
  followersError: Error | null;
  followingsError: Error | null;

  isFollowPending: boolean;

  onTabChange: (tab: string) => void;
  onArticlesClick: () => void;
  onFollowClick: () => void;
  onArticlesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}


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
