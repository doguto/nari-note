import { BookOpen, FileText, Heart } from 'lucide-react';
import { ArticleList, CourseList } from '@/components/organisms';
import { Pagination } from '@/components/molecules';
import { UserFollowersList } from './UserFollowersList';
import type { GetArticlesByAuthorResponse, GetCoursesByAuthorResponse, GetLikedArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface ProfileTabContentProps {
  activeTab: string;
  basePath: string;
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
  onArticlesRetry: () => void;
  onCoursesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}

/**
 * ProfileTabContent - Organism Component
 *
 * 選択中のタブに応じた一覧とページネーション
 */
export function ProfileTabContent({
  activeTab,
  basePath,
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
  onArticlesRetry,
  onCoursesRetry,
  onLikedArticlesRetry,
  onFollowersRetry,
  onFollowingsRetry,
}: ProfileTabContentProps) {
  const renderPagination = (totalCount: number | undefined) => (
    <div className="mt-6">
      <Pagination
        currentPage={currentPage}
        totalCount={totalCount ?? 0}
        pageSize={pageSize}
        basePath={basePath}
        query={{ tab: activeTab }}
      />
    </div>
  );

  return (
    <div>
      {activeTab === 'articles' && (
        <>
          <ArticleList
            articles={articlesData?.articles}
            isLoading={isArticlesLoading}
            error={articlesError}
            onRetry={onArticlesRetry}
            emptyMessage="まだ記事がありません"
            emptyIcon={<FileText />}
            layout="grid"
            showAuthor={false}
          />
          {!isArticlesLoading && !articlesError && renderPagination(articlesData?.totalCount)}
        </>
      )}

      {activeTab === 'courses' && (
        <>
          <CourseList
            courses={coursesData?.courses}
            isLoading={isCoursesLoading}
            error={coursesError}
            onRetry={onCoursesRetry}
            emptyIcon={<BookOpen />}
            layout="grid"
            showAuthor={false}
          />
          {!isCoursesLoading && !coursesError && renderPagination(coursesData?.totalCount)}
        </>
      )}

      {activeTab === 'likes' && (
        <>
          <ArticleList
            articles={likedArticlesData?.articles}
            isLoading={isLikedArticlesLoading}
            error={likedArticlesError}
            onRetry={onLikedArticlesRetry}
            emptyMessage="いいねした記事がありません"
            emptyIcon={<Heart />}
            layout="grid"
          />
          {!isLikedArticlesLoading && !likedArticlesError && renderPagination(likedArticlesData?.totalCount)}
        </>
      )}

      {(activeTab === 'followers' || activeTab === 'followings') && (
        <>
          {activeTab === 'followers' ? (
            <UserFollowersList
              users={followersData?.followers}
              isLoading={isFollowersLoading}
              error={followersError}
              onRetry={onFollowersRetry}
              loadingText="フォロワーを読み込み中..."
              errorMessage="フォロワーの取得に失敗しました"
              emptyMessage="フォロワーがいません"
            />
          ) : (
            <UserFollowersList
              users={followingsData?.followings}
              isLoading={isFollowingsLoading}
              error={followingsError}
              onRetry={onFollowingsRetry}
              loadingText="フォロー中のユーザーを読み込み中..."
              errorMessage="フォロー中のユーザーの取得に失敗しました"
              emptyMessage="フォロー中のユーザーがいません"
            />
          )}
        </>
      )}
      {activeTab === 'followers' && !isFollowersLoading && !followersError && renderPagination(followersData?.totalCount)}
      {activeTab === 'followings' && !isFollowingsLoading && !followingsError && renderPagination(followingsData?.totalCount)}
    </div>
  );
}
