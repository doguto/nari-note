'use client';

import { ArticleCard, CourseCard } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import { ArticleDto, CourseDto } from '@/lib/api/types';

interface HomeArticleListTemplateProps {
  articles: ArticleDto[];
}

interface HomeCourseListTemplateProps {
  courses: CourseDto[];
}

/**
 * HomeArticleListTemplate - Template Component
 * 
 * ホーム画面の記事一覧のUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function HomeArticleListTemplate({ articles }: HomeArticleListTemplateProps) {
  if (articles.length === 0) {
    return <EmptyState title="まだ記事がありません" />;
  }

  return (
    <div className="space-y-4">
      {articles.map((article) => (
        <ArticleCard
          key={article.id}
          id={article.id!}
          title={article.title ?? ''}
          author={article.authorName ?? ''}
          authorId={article.authorId ?? 0}
          tags={article.tags ?? []}
          likeCount={article.likeCount ?? 0}
          date={article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
        />
      ))}
    </div>
  );
}

/**
 * HomeCourseListTemplate - Template Component
 * 
 * ホーム画面の講座一覧のUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function HomeCourseListTemplate({ courses }: HomeCourseListTemplateProps) {
  if (courses.length === 0) {
    return <EmptyState title="まだ講座がありません" />;
  }

  return (
    <div className="space-y-4">
      {courses.map((course) => (
        <CourseCard
          key={course.id}
          id={course.id!}
          name={course.name ?? ''}
          userId={course.userId ?? 0}
          articleCount={course.articleIds?.length ?? 0}
          likeCount={course.likeCount ?? 0}
        />
      ))}
    </div>
  );
}
