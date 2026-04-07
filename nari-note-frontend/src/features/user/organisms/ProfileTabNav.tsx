import { ArticleList } from '@/components/organisms';
import { UserFollowersList } from './UserFollowersList';
import type { GetArticlesResponse, GetFollowersResponse, GetFollowingsResponse } from '@/lib/api/types';

interface ProfileTabNavProps {
  activeTab: string;
  tabContext: 'content' | 'follow';
  articlesData?: GetArticlesResponse;
  likedArticlesData?: GetArticlesResponse;
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
  onTabChange: (tab: string) => void;
  onArticlesRetry: () => void;
  onLikedArticlesRetry: () => void;
  onFollowersRetry: () => void;
  onFollowingsRetry: () => void;
}

/**
 * ProfileTabNav - Organism Component
 *
 * プロフィールページのタブナビゲーションとタブコンテンツ
 * コンテンツタブ（記事/いいね/フォロー中のタグ）とフォロータブ（フォロワー/フォロー中）を管理
 */
export function ProfileTabNav({
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
  onTabChange,
  onArticlesRetry,
  onLikedArticlesRetry,
  onFollowersRetry,
  onFollowingsRetry,
}: ProfileTabNavProps) {
  return (
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
      </div>
    </div>
  );
}
