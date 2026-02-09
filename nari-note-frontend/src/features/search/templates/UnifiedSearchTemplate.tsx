'use client';

import { SearchBar } from '@/components/molecules';
import { ArticleCard, CourseCard } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Search } from 'lucide-react';
import { ArticleDto, CourseDto } from '@/lib/api/types';

interface UnifiedSearchTemplateProps {
  keyword: string;
  searchKeyword: string;
  hasSearched: boolean;
  hasArticleResults: boolean;
  hasCourseResults: boolean;
  articles: ArticleDto[];
  courses: CourseDto[];
  onKeywordChange: (keyword: string) => void;
  onSearch: () => void;
}

/**
 * UnifiedSearchTemplate - Template Component
 * 
 * 統合検索ページ（記事・講座）のUI構成とレイアウトを担当
 * Tabs を使用して記事検索と講座検索を切り替え可能
 */
export function UnifiedSearchTemplate({
  keyword,
  searchKeyword,
  hasSearched,
  hasArticleResults,
  hasCourseResults,
  articles,
  courses,
  onKeywordChange,
  onSearch,
}: UnifiedSearchTemplateProps) {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
        検索
      </h1>
      
      <SearchBar 
        value={keyword} 
        onChange={onKeywordChange}
        onSearch={onSearch}
      />

      {/* タブで記事検索と講座検索を切り替え */}
      <Tabs defaultValue="articles" className="mt-8">
        <TabsList className="grid w-full max-w-md mx-auto grid-cols-2">
          <TabsTrigger value="articles">記事</TabsTrigger>
          <TabsTrigger value="courses">講座</TabsTrigger>
        </TabsList>

        {/* 記事検索タブ */}
        <TabsContent value="articles">
          {/* 初期状態: まだ検索していない */}
          {!hasSearched && (
            <EmptyState
              icon={<Search />}
              title="記事を検索してください"
              description="タイトルや本文から記事を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
            />
          )}

          {/* 検索結果が0件 */}
          {hasSearched && !hasArticleResults && (
            <EmptyState
              icon={<Search />}
              title="検索結果が見つかりませんでした"
              description={`「${searchKeyword}」に一致する記事が見つかりませんでした。別のキーワードで検索してみてください。`}
            />
          )}

          {/* 検索結果表示 */}
          {hasSearched && hasArticleResults && (
            <>
              {/* 検索結果件数 */}
              <div className="mb-4">
                <p className="text-gray-600">
                  {articles.length}件の記事が見つかりました
                </p>
              </div>

              {/* 検索結果一覧 */}
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
                    date={article.publishedAt 
                      ? new Date(article.publishedAt).toLocaleDateString('ja-JP') 
                      : article.createdAt 
                        ? new Date(article.createdAt).toLocaleDateString('ja-JP') 
                        : ''
                    }
                  />
                ))}
              </div>
            </>
          )}
        </TabsContent>

        {/* 講座検索タブ */}
        <TabsContent value="courses">
          {/* 初期状態: まだ検索していない */}
          {!hasSearched && (
            <EmptyState
              icon={<Search />}
              title="講座を検索してください"
              description="講座名から講座を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
            />
          )}

          {/* 検索結果が0件 */}
          {hasSearched && !hasCourseResults && (
            <EmptyState
              icon={<Search />}
              title="検索結果が見つかりませんでした"
              description={`「${searchKeyword}」に一致する講座が見つかりませんでした。別のキーワードで検索してみてください。`}
            />
          )}

          {/* 検索結果表示 */}
          {hasSearched && hasCourseResults && (
            <>
              {/* 検索結果件数 */}
              <div className="mb-4">
                <p className="text-gray-600">
                  {courses.length}件の講座が見つかりました
                </p>
              </div>

              {/* 検索結果一覧 */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {courses.map((course) => (
                  <CourseCard
                    key={course.id}
                    id={course.id!}
                    name={course.name ?? ''}
                    userId={course.userId ?? 0}
                    userName={course.userName ?? ''}
                    articleCount={course.articleIds?.length ?? 0}
                    likeCount={course.likeCount ?? 0}
                  />
                ))}
              </div>
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
