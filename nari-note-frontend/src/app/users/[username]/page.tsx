'use client';

import { useState, use } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { ArticleCard } from '@/components/common/ArticleCard';
import { Pagination } from '@/components/common/Pagination';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { mockUsers, mockArticles, currentUser } from '@/lib/mockData';
import { Users, FileText } from 'lucide-react';

export default function UserProfilePage({ params }: { params: Promise<{ username: string }> }) {
  const { username } = use(params);
  const user = mockUsers.find(u => u.username === username);
  const [currentPage, setCurrentPage] = useState(1);
  const [isFollowing, setIsFollowing] = useState(false);
  const [showFollowersDialog, setShowFollowersDialog] = useState(false);
  const [showFollowingDialog, setShowFollowingDialog] = useState(false);
  const articlesPerPage = 20;

  if (!user) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4">ユーザーが見つかりません</h1>
        <Link href="/">
          <Button>トップページに戻る</Button>
        </Link>
      </div>
    );
  }

  // ユーザーの記事を取得
  const userArticles = mockArticles.filter(article => article.authorId === user.id);
  const totalPages = Math.ceil(userArticles.length / articlesPerPage);
  const indexOfLastArticle = currentPage * articlesPerPage;
  const indexOfFirstArticle = indexOfLastArticle - articlesPerPage;
  const currentArticles = userArticles.slice(indexOfFirstArticle, indexOfLastArticle);

  const isOwnProfile = user.id === currentUser.id;

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* プロフィールヘッダー */}
        <Card className="mb-8">
          <CardContent className="pt-6">
            <div className="flex items-start gap-6">
              <Avatar className="h-24 w-24">
                <AvatarImage src={user.avatarUrl} alt={user.displayName} />
                <AvatarFallback className="text-3xl">{user.displayName[0]}</AvatarFallback>
              </Avatar>

              <div className="flex-1">
                <div className="flex justify-between items-start mb-2">
                  <div>
                    <h1 className="text-3xl font-bold">{user.displayName}</h1>
                    <p className="text-gray-500">@{user.username}</p>
                  </div>
                  {!isOwnProfile && (
                    <Button
                      variant={isFollowing ? 'outline' : 'default'}
                      onClick={() => setIsFollowing(!isFollowing)}
                    >
                      {isFollowing ? 'フォロー中' : 'フォローする'}
                    </Button>
                  )}
                </div>

                {user.bio && (
                  <p className="text-gray-700 mb-4">{user.bio}</p>
                )}

                <div className="flex gap-4">
                  <button
                    onClick={() => setShowFollowingDialog(true)}
                    className="hover:underline"
                  >
                    <span className="font-semibold">{user.followingCount}</span>
                    <span className="text-gray-600 ml-1">フォロー</span>
                  </button>
                  <button
                    onClick={() => setShowFollowersDialog(true)}
                    className="hover:underline"
                  >
                    <span className="font-semibold">{user.followersCount}</span>
                    <span className="text-gray-600 ml-1">フォロワー</span>
                  </button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Separator className="my-8" />

        {/* 記事一覧 */}
        <div className="mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <FileText className="w-6 h-6" />
            投稿記事 ({userArticles.length})
          </h2>
        </div>

        {userArticles.length === 0 ? (
          <Card>
            <CardContent className="pt-6 text-center py-12">
              <p className="text-gray-500">まだ記事が投稿されていません</p>
            </CardContent>
          </Card>
        ) : (
          <>
            <div className="grid gap-6 mb-8">
              {currentArticles.map((article) => (
                <ArticleCard key={article.id} article={article} />
              ))}
            </div>

            {totalPages > 1 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
              />
            )}
          </>
        )}

        {/* フォロワー一覧ダイアログ */}
        <Dialog open={showFollowersDialog} onOpenChange={setShowFollowersDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>フォロワー</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              {mockUsers.slice(0, 5).map((follower) => (
                <div key={follower.id} className="flex items-center justify-between">
                  <Link href={`/users/${follower.username}`} className="flex items-center gap-3">
                    <Avatar>
                      <AvatarImage src={follower.avatarUrl} alt={follower.displayName} />
                      <AvatarFallback>{follower.displayName[0]}</AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-semibold">{follower.displayName}</p>
                      <p className="text-sm text-gray-500">@{follower.username}</p>
                    </div>
                  </Link>
                  <Button variant="outline" size="sm">
                    フォロー
                  </Button>
                </div>
              ))}
            </div>
          </DialogContent>
        </Dialog>

        {/* フォロー一覧ダイアログ */}
        <Dialog open={showFollowingDialog} onOpenChange={setShowFollowingDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>フォロー中</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              {mockUsers.slice(0, 3).map((following) => (
                <div key={following.id} className="flex items-center justify-between">
                  <Link href={`/users/${following.username}`} className="flex items-center gap-3">
                    <Avatar>
                      <AvatarImage src={following.avatarUrl} alt={following.displayName} />
                      <AvatarFallback>{following.displayName[0]}</AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-semibold">{following.displayName}</p>
                      <p className="text-sm text-gray-500">@{following.username}</p>
                    </div>
                  </Link>
                  <Button variant="outline" size="sm">
                    フォロー中
                  </Button>
                </div>
              ))}
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
