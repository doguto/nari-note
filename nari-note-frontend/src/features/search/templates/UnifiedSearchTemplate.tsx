import { useState } from 'react';
import { SearchBar } from '@/components/molecules';
import { ArticleCard, CourseCard } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Search, ChevronDown } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ArticleThumbnailDto, CourseDto } from '@/lib/api/types';

interface UnifiedSearchTemplateProps {
  keyword: string;
  searchKeyword: string;
  hasSearched: boolean;
  hasArticleResults: boolean;
  hasCourseResults: boolean;
  articles: ArticleThumbnailDto[];
  courses: CourseDto[];
  onKeywordChange: (keyword: string) => void;
  onSearch: () => void;
}


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
  const [activeTab, setActiveTab] = useState<'articles' | 'courses'>('articles');

  return (
    <div className="container mx-auto px-4 py-8">
      <div className='flex justify-center w-full max-w-2xl mx-auto'>
        <SearchBar
          value={keyword}
          onChange={onKeywordChange}
          onSearch={onSearch}
        />
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as 'articles' | 'courses')} className="flex flex-col gap-6">
        {/* モバイル: カスタムドロップダウン */}
        <div className="block md:hidden w-full max-w-2xl mx-auto">
          <DropdownMenu>
            <DropdownMenuTrigger className="flex w-full items-center justify-between rounded-md border border-brand-border bg-white px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring">
              {activeTab === 'articles' ? '記事' : '講座'}
              <ChevronDown className="h-4 w-4 opacity-50" />
            </DropdownMenuTrigger>
            <DropdownMenuContent style={{ width: 'var(--radix-dropdown-menu-trigger-width)' }}>
              <DropdownMenuItem onSelect={() => setActiveTab('articles')}>記事</DropdownMenuItem>
              <DropdownMenuItem onSelect={() => setActiveTab('courses')}>講座</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        {/* デスクトップ: Tabs */}
        <TabsList className="hidden md:flex w-full max-w-2xl mx-auto">
          <TabsTrigger value="articles" className="flex-1">記事</TabsTrigger>
          <TabsTrigger value="courses" className="flex-1">講座</TabsTrigger>
        </TabsList>

        <div className="w-full max-w-2xl mx-auto">
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
                        : article.updatedAt
                          ? new Date(article.updatedAt).toLocaleDateString('ja-JP')
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
