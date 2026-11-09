import { ArticleList, CourseList } from '@/components/organisms';
import { Pagination } from '@/components/molecules';
import { Button } from '@/components/ui/button';
import { UserFollowersList } from './UserFollowersList';
import type { GetArticlesByAuthorResponse, GetCoursesByAuthorResponse, GetLikedArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface ProfileTabNavProps {
  activeTab: string;
  tabContext: 'content' | 'follow';
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
  onTabChange: (tab: string) => void;
  onArticlesRetry: () => void;
  onCoursesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}

/**
 * ProfileTabNav - Organism Component
 *
 * プロフィールページのタブナビゲーションとタブコンテンツ
 * コンテンツタブ（記事/講座/いいね/フォロー中のタグ）とフォロータブ（フォロワー/フォロー中）を管理
 */
export function ProfileTabNav({
  activeTab,
  tabContext,
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
  onTabChange,
  onArticlesRetry,
  onCoursesRetry,
  onLikedArticlesRetry,
  onFollowersRetry,
  onFollowingsRetry,
}: ProfileTabNavProps) {
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
    <div className="bg-white rounded-lg shadow">
      <div className="border-b border-gray-200">
        <nav className="flex gap-6 sm:gap-8 px-4 sm:px-6 overflow-x-auto">
          {tabContext === 'content' ? (
            <>
              {/* コンテンツタブ（記事/講座/いいね/フォロー中のタグ） */}
              <Button
                variant="ghost"
                onClick={() => onTabChange('articles')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'articles'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                記事
              </Button>
              <Button
                variant="ghost"
                onClick={() => onTabChange('courses')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'courses'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                講座
              </Button>
              <Button
                variant="ghost"
                onClick={() => onTabChange('likes')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'likes'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                いいね
              </Button>
              <Button
                variant="ghost"
                onClick={() => onTabChange('following-tags')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'following-tags'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                フォロー中のタグ
              </Button>
            </>
          ) : (
            <>
              {/* フォロータブ（フォロワー/フォロー中） */}
              <Button
                variant="ghost"
                onClick={() => onTabChange('followers')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'followers'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                フォロワー
              </Button>
              <Button
                variant="ghost"
                onClick={() => onTabChange('followings')}
                className={`py-4 h-auto px-0 border-b-2 whitespace-nowrap flex-shrink-0 rounded-none hover:bg-transparent ${
                  activeTab === 'followings'
                    ? 'border-brand-primary text-brand-text font-medium'
                    : 'border-transparent text-gray-600 hover:text-brand-text'
                }`}
              >
                フォロー中
              </Button>
            </>
          )}
        </nav>
      </div>

      {/* タブコンテンツ */}
      <div className="p-4 sm:p-6">
        {activeTab === 'articles' && (
          <ArticleList
            articles={articlesData?.articles}
            isLoading={isArticlesLoading}
            error={articlesError}
            onRetry={onArticlesRetry}
            emptyMessage="まだ記事がありません"
          />
        )}
        {activeTab === 'articles' && !isArticlesLoading && !articlesError && renderPagination(articlesData?.totalCount)}

        {activeTab === 'courses' && (
          <CourseList
            courses={coursesData?.courses}
            isLoading={isCoursesLoading}
            error={coursesError}
            onRetry={onCoursesRetry}
          />
        )}
        {activeTab === 'courses' && !isCoursesLoading && !coursesError && renderPagination(coursesData?.totalCount)}

        {activeTab === 'likes' && (
          <ArticleList
            articles={likedArticlesData?.articles}
            isLoading={isLikedArticlesLoading}
            error={likedArticlesError}
            onRetry={onLikedArticlesRetry}
            emptyMessage="いいねした記事がありません"
          />
        )}
        {activeTab === 'likes' && !isLikedArticlesLoading && !likedArticlesError && renderPagination(likedArticlesData?.totalCount)}

        {activeTab === 'following-tags' && (
          <div className="text-center py-8 text-gray-500">
            フォロー中のタグ一覧は今後実装予定です
          </div>
        )}

        {activeTab === 'followers' && (
          <UserFollowersList
            users={followersData?.followers}
            isLoading={isFollowersLoading}
            error={followersError}
            onRetry={onFollowersRetry}
            loadingText="フォロワーを読み込み中..."
            errorMessage="フォロワーの取得に失敗しました"
            emptyMessage="フォロワーがいません"
          />
        )}
        {activeTab === 'followers' && !isFollowersLoading && !followersError && renderPagination(followersData?.totalCount)}

        {activeTab === 'followings' && (
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
        {activeTab === 'followings' && !isFollowingsLoading && !followingsError && renderPagination(followingsData?.totalCount)}
      </div>
    </div>
  );
}
