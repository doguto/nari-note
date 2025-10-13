'use client';

import { useState, use } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { mockArticles, currentUser } from '@/lib/mockData';
import { formatDistanceToNow } from 'date-fns';
import { ja } from 'date-fns/locale';
import { Heart, MessageCircle, Edit } from 'lucide-react';
import { MarkdownContent } from '@/components/common/MarkdownContent';

export default function ArticlePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const article = mockArticles.find(a => a.id === id);
  const [isLiked, setIsLiked] = useState(article?.isLiked || false);
  const [likesCount, setLikesCount] = useState(article?.likes || 0);

  if (!article) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4">記事が見つかりません</h1>
        <Link href="/">
          <Button>トップページに戻る</Button>
        </Link>
      </div>
    );
  }

  const formattedDate = formatDistanceToNow(new Date(article.createdAt), {
    addSuffix: true,
    locale: ja,
  });

  const handleLike = () => {
    if (isLiked) {
      setLikesCount(likesCount - 1);
    } else {
      setLikesCount(likesCount + 1);
    }
    setIsLiked(!isLiked);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* タイトル */}
        <div className="flex justify-between items-start mb-6">
          <h1 className="text-4xl font-bold flex-1">{article.title}</h1>
          {article.authorId === currentUser.id && (
            <Link href={`/articles/${article.id}/edit`}>
              <Button variant="outline" size="sm" className="flex items-center gap-2">
                <Edit className="w-4 h-4" />
                編集
              </Button>
            </Link>
          )}
        </div>

        {/* 著者情報 */}
        <div className="flex items-center justify-between mb-6">
          <Link
            href={`/users/${article.author.username}`}
            className="flex items-center gap-3 hover:opacity-80"
          >
            <Avatar className="h-12 w-12">
              <AvatarImage src={article.author.avatarUrl} alt={article.author.displayName} />
              <AvatarFallback>{article.author.displayName[0]}</AvatarFallback>
            </Avatar>
            <div>
              <p className="font-semibold">{article.author.displayName}</p>
              <p className="text-sm text-gray-500">{formattedDate}</p>
            </div>
          </Link>
          
          <Button variant="outline">フォロー</Button>
        </div>

        {/* タグ */}
        <div className="flex flex-wrap gap-2 mb-6">
          {article.tags.map((tag) => (
            <Link key={tag.id} href={`/tags/${tag.name}`}>
              <Badge variant="secondary" className="hover:bg-gray-300 cursor-pointer">
                #{tag.name}
              </Badge>
            </Link>
          ))}
        </div>

        {/* 記事本文 */}
        <Card className="mb-6 wa-pattern">
          <CardContent className="pt-6">
            <MarkdownContent content={article.content} />
          </CardContent>
        </Card>

        {/* いいねとコメント */}
        <div className="flex items-center gap-4 mb-6">
          <Button
            variant={isLiked ? 'default' : 'outline'}
            onClick={handleLike}
            className="flex items-center gap-2"
          >
            <Heart className={`w-5 h-5 ${isLiked ? 'fill-current' : ''}`} />
            {likesCount}
          </Button>
          <Button variant="outline" className="flex items-center gap-2">
            <MessageCircle className="w-5 h-5" />
            {article.commentCount}
          </Button>
        </div>

        <Separator className="my-8" />

        {/* 著者の他の記事 */}
        <div className="mt-8">
          <h2 className="text-2xl font-bold mb-4">著者の他の記事</h2>
          <div className="grid gap-4">
            {mockArticles
              .filter(a => a.authorId === article.authorId && a.id !== article.id)
              .slice(0, 3)
              .map(relatedArticle => (
                <Link key={relatedArticle.id} href={`/articles/${relatedArticle.id}`}>
                  <Card className="hover:shadow-lg transition-shadow">
                    <CardContent className="pt-6">
                      <h3 className="font-bold hover:text-blue-600">{relatedArticle.title}</h3>
                      <div className="flex items-center gap-4 mt-2 text-sm text-gray-600">
                        <span className="flex items-center gap-1">
                          <Heart className="w-4 h-4" />
                          {relatedArticle.likes}
                        </span>
                        <span className="flex items-center gap-1">
                          <MessageCircle className="w-4 h-4" />
                          {relatedArticle.commentCount}
                        </span>
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              ))}
          </div>
        </div>
      </div>
    </div>
  );
}
