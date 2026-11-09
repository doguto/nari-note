import Link from 'next/link';
import { ArticleCard, CourseCard, Pagination } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import type { ArticleThumbnailDto, CourseDto } from '@/lib/api/types';

export type TagTab = 'articles' | 'courses';

interface TagArticleListTemplateProps {
  tag: string;
  activeTab: TagTab;
  articles: ArticleThumbnailDto[];
  courses: CourseDto[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

/**
 * TagArticleListTemplate - Template Component
 *
 * タグ別一覧ページのUI構成を担当するテンプレートコンポーネント
 * 記事と講座をタブで切り替えて表示する（タブは URL の `?tab=` で管理する）
 */
export function TagArticleListTemplate({
  tag,
  activeTab,
  articles,
  courses,
  totalCount,
  currentPage,
  pageSize,
}: TagArticleListTemplateProps) {
  const basePath = `/tags/${encodeURIComponent(tag)}`;
  const tabQuery: Record<string, string> = activeTab === 'courses' ? { tab: 'courses' } : {};

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-3xl font-bold text-brand-text mb-2">
          #{tag}
        </h1>
        <p className="text-gray-600">
          {totalCount}件の{activeTab === 'articles' ? '記事' : '講座'}
        </p>
      </div>

      <Tabs value={activeTab}>
        <TabsList className="flex w-full">
          <TabsTrigger value="articles" className="flex-1" asChild>
            <Link href={basePath}>記事</Link>
          </TabsTrigger>
          <TabsTrigger value="courses" className="flex-1" asChild>
            <Link href={`${basePath}?tab=courses`}>講座</Link>
          </TabsTrigger>
        </TabsList>
      </Tabs>

      {activeTab === 'articles' && (
        totalCount === 0 ? (
          <EmptyState
            icon="📝"
            title="記事がありません"
            description={`#${tag} に関連する記事はまだ投稿されていません。`}
          />
        ) : (
          <div className="space-y-4">
            {articles.map((article) => (
              <ArticleCard
                key={article.id}
                id={article.id!}
                title={article.title ?? ''}
                author={article.authorName ?? ''}
                authorId={article.authorId ?? 0}
                userIconImageUrl={article.userIconImageUrl}
                tags={article.tags ?? []}
                likeCount={article.likeCount ?? 0}
                date={article.publishedAt ? new Date(article.publishedAt).toLocaleDateString('ja-JP') : article.updatedAt ? new Date(article.updatedAt).toLocaleDateString('ja-JP') : ''}
              />
            ))}
          </div>
        )
      )}

      {activeTab === 'courses' && (
        totalCount === 0 ? (
          <EmptyState
            icon="📚"
            title="講座がありません"
            description={`#${tag} の記事を含む講座はまだ公開されていません。`}
          />
        ) : (
          <div className="space-y-4">
            {courses.map((course) => (
              <CourseCard
                key={course.id}
                id={course.id}
                name={course.name ?? ''}
                userId={course.userId ?? 0}
                userName={course.userName ?? ''}
                userIconImageUrl={course.userIconImageUrl}
                articleCount={course.articleIds?.length ?? 0}
                likeCount={course.likeCount ?? 0}
              />
            ))}
          </div>
        )
      )}

      <Pagination
        currentPage={currentPage}
        totalCount={totalCount}
        pageSize={pageSize}
        basePath={basePath}
        query={tabQuery}
      />
    </div>
  );
}
