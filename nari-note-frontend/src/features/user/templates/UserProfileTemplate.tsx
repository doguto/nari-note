import { ProfileCard, ProfileTabs, ProfileTabContent } from '../organisms';
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
  isFollowPending,
  onTabChange,
  onFollowClick,
  ...contentProps
}: UserProfileTemplateProps) {
  return (
    <div className="space-y-6">
      <section className="overflow-hidden rounded-xl bg-white shadow-sm">
        <ProfileCard
          user={user}
          isOwnProfile={isOwnProfile}
          isFollowPending={isFollowPending}
          onTabChange={onTabChange}
          onFollowClick={onFollowClick}
        />
        <ProfileTabs activeTab={activeTab} tabContext={tabContext} onTabChange={onTabChange} />
      </section>

      <ProfileTabContent
        activeTab={activeTab}
        basePath={`/users/${user.id}`}
        {...contentProps}
      />
    </div>
  );
}
