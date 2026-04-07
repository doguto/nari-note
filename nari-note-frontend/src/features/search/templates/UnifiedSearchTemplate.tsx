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
      <div className='flex justify-center w-full max-w-2xl mx-auto mb-8'>
        <SearchBar
          value={keyword} 
          onChange={onKeywordChange}
          onSearch={onSearch}
        />
      </div>
      
      {/* タブを左に縦並びで配置 */}
      <Tabs defaultValue="articles" className="flex flex-col md:flex-row gap-6">
        <TabsList className="flex flex-col md:flex-col h-fit md:w-48 gap-2">
          <TabsTrigger value="articles" className="w-full justify-start">記事</TabsTrigger>
          <TabsTrigger value="courses" className="w-full justify-start">講座</TabsTrigger>
        </TabsList>

        <div className="flex-1">
          <TabsContent value="articles">
            {!hasSearched && (
              <EmptyState
                icon={<Search />}
                title="記事を検索してください"
                description="タイトルや本文から記事を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
              />
            )}

            {hasSearched && !hasArticleResults && (
              <EmptyState
                icon={<Search />}
                title="検索結果が見つかりませんでした"
                description={`「${searchKeyword}」に一致する記事が見つかりませんでした。別のキーワードで検索してみてください。`}
              />
            )}

            {hasSearched && hasArticleResults && (
              <>
                <div className="mb-4">
                  <p className="text-gray-600">
                    {articles.length}件の記事が見つかりました
                  </p>
                </div>

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

          <TabsContent value="courses">
            {!hasSearched && (
              <EmptyState
                icon={<Search />}
                title="講座を検索してください"
                description="講座名から講座を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
              />
            )}

            {hasSearched && !hasCourseResults && (
              <EmptyState
                icon={<Search />}
                title="検索結果が見つかりませんでした"
                description={`「${searchKeyword}」に一致する講座が見つかりませんでした。別のキーワードで検索してみてください。`}
              />
            )}

            {hasSearched && hasCourseResults && (
              <>
                <div className="mb-4">
                  <p className="text-gray-600">
                    {courses.length}件の講座が見つかりました
                  </p>
                </div>

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
        </div>
      </Tabs>
    </div>
  );
}
