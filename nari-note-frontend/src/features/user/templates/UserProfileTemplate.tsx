import { ProfileCard } from '../organisms/ProfileCard';
import { ProfileTabNav } from '../organisms/ProfileTabNav';
import type { GetUserProfileResponse, GetArticlesByAuthorResponse, GetCoursesByAuthorResponse, GetLikedArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface UserProfileTemplateProps {
  user: GetUserProfileResponse;
  isOwnProfile: boolean;

  activeTab: string;
  tabContext: 'content' | 'follow';
  currentPage: number;
  pageSize: number;

  articlesData?: GetArticlesByAuthorResponse;
  coursesData?: GetCoursesByAuthorResponse;
  likedArticlesData?: GetLikedArticlesResponse;
  followersData?: GetFollowersResponse;
  followingsData?: GetFollowingsResponse;

  isArticlesLoading: boolean;
  isCoursesLoading: boolean;
  isLikedArticlesLoading: boolean;
  isFollowersLoading: boolean;
  isFollowingsLoading: boolean;

  articlesError: Error | null;
  coursesError: Error | null;
  likedArticlesError: Error | null;
  followersError: Error | null;
  followingsError: Error | null;

  isFollowPending: boolean;

  onTabChange: (tab: string) => void;
  onArticlesClick: () => void;
  onFollowClick: () => void;
  onArticlesRetry: () => void;
  onCoursesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}


export function UserProfileTemplate({
  user,
  isOwnProfile,
  activeTab,
  tabContext,
  currentPage,
  pageSize,
  articlesData,
  coursesData,
  likedArticlesData,
  followersData,
  followingsData,
  isArticlesLoading,
  isCoursesLoading,
  isLikedArticlesLoading,
  isFollowersLoading,
  isFollowingsLoading,
  articlesError,
  coursesError,
  likedArticlesError,
  followersError,
  followingsError,
  isFollowPending,
  onTabChange,
  onArticlesClick,
  onFollowClick,
  onArticlesRetry,
  onCoursesRetry,
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
        basePath={`/users/${user.id}`}
        currentPage={currentPage}
        pageSize={pageSize}
        articlesData={articlesData}
        coursesData={coursesData}
        likedArticlesData={likedArticlesData}
        followersData={followersData}
        followingsData={followingsData}
        isArticlesLoading={isArticlesLoading}
        isCoursesLoading={isCoursesLoading}
        isLikedArticlesLoading={isLikedArticlesLoading}
        isFollowersLoading={isFollowersLoading}
        isFollowingsLoading={isFollowingsLoading}
        articlesError={articlesError}
        coursesError={coursesError}
        likedArticlesError={likedArticlesError}
        followersError={followersError}
        followingsError={followingsError}
        onTabChange={onTabChange}
        onArticlesRetry={onArticlesRetry}
        onCoursesRetry={onCoursesRetry}
        onLikedArticlesRetry={onLikedArticlesRetry}
        onFollowersRetry={onFollowersRetry}
        onFollowingsRetry={onFollowingsRetry}
      />
    </div>
  );
}
