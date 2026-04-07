'use client';

import { useState } from 'react';
import { useSearchArticles, useSearchCourses } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { UnifiedSearchTemplate } from '../templates/UnifiedSearchTemplate';

/**
 * UnifiedSearchPage - Page Component
 * 
 * 統合検索ページ（記事・講座）のロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function UnifiedSearchPage() {
  const [keyword, setKeyword] = useState('');
  const [searchKeyword, setSearchKeyword] = useState('');

  // 検索実行: 検索ボタンクリックまたはEnterキーで実行
  const handleSearch = () => {
    if (keyword.trim()) {
      setSearchKeyword(keyword.trim());
    }
  };

  // 記事検索
  const { 
    data: articleData, 
    isLoading: articleLoading, 
    error: articleError, 
    refetch: refetchArticles 
  } = useSearchArticles(
    { keyword: searchKeyword, limit: 20, offset: 0 },
    { enabled: searchKeyword.length > 0 }
  );

  // 講座検索
  const { 
    data: courseData, 
    isLoading: courseLoading, 
    error: courseError, 
    refetch: refetchCourses 
  } = useSearchCourses(
    { keyword: searchKeyword, limit: 20, offset: 0 },
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
    <UnifiedSearchTemplate
      keyword={keyword}
      searchKeyword={searchKeyword}
      hasSearched={hasSearched}
      hasArticleResults={hasArticleResults}
      hasCourseResults={hasCourseResults}
      articles={articlesWithId}
      courses={coursesWithId}
      onKeywordChange={setKeyword}
      onSearch={handleSearch}
    />
  );
}
