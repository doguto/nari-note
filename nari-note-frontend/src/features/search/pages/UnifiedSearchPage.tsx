'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useSearchArticles, useSearchCourses } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { PageWithSidebar } from '@/features/global/organisms';
import { UnifiedSearchTemplate, type SearchTab } from '../templates/UnifiedSearchTemplate';

const PAGE_SIZE = 20;

interface UnifiedSearchPageProps {
  searchKeyword: string;
  activeTab: SearchTab;
  page: number;
}

/**
 * 検索結果ページの URL を組み立てる（1 ページ目と記事タブは既定値なので省略する）
 */
function buildSearchQuery(keyword: string, tab: SearchTab): Record<string, string> {
  const query: Record<string, string> = {};
  if (keyword) query.q = keyword;
  if (tab === 'courses') query.tab = tab;
  return query;
}

/**
 * UnifiedSearchPage - Page Component
 * 
 * 統合検索ページ（記事・講座）のロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 * キーワード・タブ・ページは URL（`?q=&tab=&page=`）で管理し、ページ送りやブラウザバックで復元できるようにする
 */
export function UnifiedSearchPage({ searchKeyword, activeTab, page }: UnifiedSearchPageProps) {
  const router = useRouter();
  const [keyword, setKeyword] = useState(searchKeyword);

  const navigate = (nextKeyword: string, tab: SearchTab) => {
    const params = new URLSearchParams(buildSearchQuery(nextKeyword, tab)).toString();
    router.push(params ? `/search?${params}` : '/search');
  };

  // 検索実行: 検索ボタンクリックまたはEnterキーで実行（1ページ目に戻す）
  const handleSearch = () => {
    if (keyword.trim()) {
      navigate(keyword.trim(), activeTab);
    }
  };

  // タブを切り替えたら 1 ページ目から表示する
  const handleTabChange = (tab: SearchTab) => {
    navigate(searchKeyword, tab);
  };

  // ページ番号は表示中のタブにだけ適用する
  const articlePage = activeTab === 'articles' ? page : 1;
  const coursePage = activeTab === 'courses' ? page : 1;

  // 記事検索
  const { 
    data: articleData, 
    isLoading: articleLoading, 
    error: articleError, 
    refetch: refetchArticles 
  } = useSearchArticles(
    { keyword: searchKeyword, limit: PAGE_SIZE, offset: (articlePage - 1) * PAGE_SIZE },
    { enabled: searchKeyword.length > 0 }
  );

  // 講座検索
  const { 
    data: courseData, 
    isLoading: courseLoading, 
    error: courseError, 
    refetch: refetchCourses 
  } = useSearchCourses(
    { keyword: searchKeyword, limit: PAGE_SIZE, offset: (coursePage - 1) * PAGE_SIZE },
    { enabled: searchKeyword.length > 0 }
  );

  // 検索結果表示の判定
  const hasSearched = searchKeyword.length > 0;
  const hasArticleResults = !!(articleData?.articles && articleData.articles.length > 0);
  const hasCourseResults = !!(courseData?.courses && courseData.courses.length > 0);

  // IDが存在しない記事・講座をフィルタリング
  const articlesWithId = (articleData?.articles || []).filter(
    (article) => article.id !== null && article.id !== undefined
  );
  const coursesWithId = (courseData?.courses || []).filter(
    (course) => course.id !== null && course.id !== undefined
  );

  // ローディング表示
  if (hasSearched && (articleLoading || courseLoading)) {
    return <LoadingSpinner text="検索中..." />;
  }

  // エラー表示（記事検索と講座検索の両方でエラーが発生した場合）
  if (hasSearched && articleError && courseError) {
    return (
      <ErrorMessage 
        message="検索に失敗しました" 
        onRetry={() => {
          refetchArticles();
          refetchCourses();
        }}
      />
    );
  }

  return (
    <PageWithSidebar>
      <UnifiedSearchTemplate
      keyword={keyword}
      searchKeyword={searchKeyword}
      hasSearched={hasSearched}
      hasArticleResults={hasArticleResults}
      hasCourseResults={hasCourseResults}
      articles={articlesWithId}
      courses={coursesWithId}
      activeTab={activeTab}
      articlePage={articlePage}
      coursePage={coursePage}
      pageSize={PAGE_SIZE}
      articleTotalCount={articleData?.totalCount ?? 0}
      courseTotalCount={courseData?.totalCount ?? 0}
      paginationQuery={buildSearchQuery(searchKeyword, activeTab)}
      onKeywordChange={setKeyword}
      onSearch={handleSearch}
      onTabChange={handleTabChange}
      />
    </PageWithSidebar>
  );
}
