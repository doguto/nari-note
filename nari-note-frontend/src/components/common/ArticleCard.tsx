import React from 'react';
import Link from 'next/link';
import { Article } from '@/types';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { formatDistanceToNow } from 'date-fns';
import { ja } from 'date-fns/locale';
import { Heart, MessageCircle } from 'lucide-react';

interface ArticleCardProps {
  article: Article;
}

export const ArticleCard: React.FC<ArticleCardProps> = ({ article }) => {
  const formattedDate = formatDistanceToNow(new Date(article.createdAt), {
    addSuffix: true,
    locale: ja,
  });

  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <Link href={`/articles/${article.id}`}>
          <h2 className="text-xl font-bold hover:text-blue-600 transition-colors">
            {article.title}
          </h2>
        </Link>
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-3 mb-3">
          <Link href={`/users/${article.author.username}`} className="flex items-center gap-2 hover:opacity-80">
            <Avatar className="h-8 w-8">
              <AvatarImage src={article.author.avatarUrl} alt={article.author.displayName} />
              <AvatarFallback>{article.author.displayName[0]}</AvatarFallback>
            </Avatar>
            <span className="text-sm text-gray-700">{article.author.displayName}</span>
          </Link>
          <span className="text-sm text-gray-500">{formattedDate}</span>
        </div>

        <div className="flex flex-wrap gap-2 mb-3">
          {article.tags.map((tag) => (
            <Link key={tag.id} href={`/tags/${tag.name}`}>
              <Badge variant="secondary" className="hover:bg-gray-300 cursor-pointer">
                #{tag.name}
              </Badge>
            </Link>
          ))}
        </div>

        <div className="flex items-center gap-4 text-sm text-gray-600">
          <span className="flex items-center gap-1">
            <Heart className={`w-5 h-5 ${article.isLiked ? 'fill-red-500 text-red-500' : ''}`} />
            {article.likes}
          </span>
          <span className="flex items-center gap-1">
            <MessageCircle className="w-5 h-5" />
            {article.commentCount}
          </span>
        </div>
      </CardContent>
    </Card>
  );
};
