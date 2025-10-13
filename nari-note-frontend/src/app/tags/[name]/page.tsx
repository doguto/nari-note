'use client';

import { useState, use } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { ArticleCard } from '@/components/common/ArticleCard';
import { Pagination } from '@/components/common/Pagination';
import { mockArticles, mockTags } from '@/lib/mockData';
import { Users, FileText } from 'lucide-react';

export default function TagTimelinePage({ params }: { params: Promise<{ name: string }> }) {
  const { name } = use(params);
  const decodedName = decodeURIComponent(name);
  
  const tag = mockTags.find(t => t.name === decodedName);
  const [currentPage, setCurrentPage] = useState(1);
  const [isFollowing, setIsFollowing] = useState(false);
  const articlesPerPage = 20;

  // このタグを持つ記事を抽出
  const tagArticles = mockArticles.filter(article =>
    article.tags.some(t => t.name === decodedName)
  ).sort((a, b) => 
    new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  const totalPages = Math.ceil(tagArticles.length / articlesPerPage);
  const indexOfLastArticle = currentPage * articlesPerPage;
  const indexOfFirstArticle = indexOfLastArticle - articlesPerPage;
  const currentArticles = tagArticles.slice(indexOfFirstArticle, indexOfLastArticle);

  if (!tag) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4">タグが見つかりません</h1>
        <Link href="/">
          <Button>トップページに戻る</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* タグヘッダー */}
        <Card className="mb-8">
          <CardContent className="pt-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h1 className="text-4xl font-bold mb-2">#{tag.name}</h1>
                {tag.description && (
                  <p className="text-gray-600 mb-4">{tag.description}</p>
                )}
              </div>
              <Button
                variant={isFollowing ? 'outline' : 'default'}
                onClick={() => setIsFollowing(!isFollowing)}
              >
                {isFollowing ? 'フォロー中' : 'フォローする'}
              </Button>
            </div>

            <div className="flex gap-6 text-sm text-gray-600">
              <div className="flex items-center gap-2">
                <FileText className="w-5 h-5" />
                <span>{tag.articleCount} 件の記事</span>
              </div>
              <div className="flex items-center gap-2">
                <Users className="w-5 h-5" />
                <span>{tag.followersCount} 人がフォロー中</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* 記事一覧 */}
        {tagArticles.length === 0 ? (
          <Card>
            <CardContent className="pt-6 text-center py-12">
              <p className="text-gray-500">このタグの記事はまだありません</p>
            </CardContent>
          </Card>
        ) : (
          <>
            <div className="mb-6">
              <h2 className="text-xl font-semibold">記事一覧（新着順）</h2>
            </div>

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
      </div>
    </div>
  );
}
