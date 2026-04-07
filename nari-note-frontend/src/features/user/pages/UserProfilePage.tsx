'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import { useGetUserProfile, useToggleFollow, useGetFollowers, useGetFollowings, useGetArticlesByAuthor, useGetLikedArticles } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { useAuth } from '@/lib/providers/AuthProvider';
import { PageWithSidebar } from '@/features/global/organisms';
import { UserProfileTemplate } from '../templates/UserProfileTemplate';

interface UserProfilePageProps {
  userId: number;
}

/**
 * UserProfilePage - Page Component
 * 
 * ユーザープロフィールページのビジネスロジックを担当するページコンポーネント
 * データフェッチング、状態管理、イベントハンドリングを行い、Templateにpropsを渡す
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
  
  // ユーザーの記事取得（タブがarticlesの場合のみ）
  const { data: articlesData, isLoading: isArticlesLoading, error: articlesError, refetch: refetchArticles } = useGetArticlesByAuthor(
    { authorId: userId },
    { enabled: activeTab === 'articles' }
  );
  
  // いいねした記事取得（タブがlikesの場合のみ）
  const { data: likedArticlesData, isLoading: isLikedArticlesLoading, error: likedArticlesError, refetch: refetchLikedArticles } = useGetLikedArticles(
    { userId: userId },
    { enabled: activeTab === 'likes' }
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
    return <LoadingSpinner text="ユーザー情報を読み込み中..." />;
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
    <PageWithSidebar>
      <UserProfileTemplate
        user={user}
        isOwnProfile={isOwnProfile}
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
        isFollowPending={isFollowPending}
        onTabChange={handleTabChange}
        onArticlesClick={handleArticlesClick}
        onFollowClick={handleFollowClick}
        onArticlesRetry={refetchArticles}
        onLikedArticlesRetry={refetchLikedArticles}
        onFollowersRetry={refetchFollowers}
        onFollowingsRetry={refetchFollowings}
      />
    </PageWithSidebar>
  );
}
