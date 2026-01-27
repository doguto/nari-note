'use client';

import { ArticleCard } from '@/components/molecules';
import { ArticleDto } from '@/lib/api/types';

interface HomeArticleListTemplateProps {
  articles: ArticleDto[];
}

/**
 * HomeArticleListTemplate - Template Component
 * 
 * ホーム画面の記事一覧のUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function HomeArticleListTemplate({ articles }: HomeArticleListTemplateProps) {
  return (
    <div className="space-y-4">
      {articles.map((article) => (
        <ArticleCard
          key={article.id}
          id={article.id!}
          title={article.title ?? ''}
          author={article.authorName ?? ''}
          authorId={article.authorId ?? 0}
          stats={`いいね ${article.likeCount ?? 0}`}
          date={article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
          image="📝"
        />
      ))}
    </div>
  );
}
